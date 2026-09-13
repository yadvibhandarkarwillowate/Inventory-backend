using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Enums;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    public async Task<PagedResult<StockLevelDto>> GetStockLevelsAsync(StockQueryParams query)
    {
        var productsQuery = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(term) || p.Sku.ToLower().Contains(term));
        }

        if (query.LowStockOnly)
            productsQuery = productsQuery.Where(p => p.CurrentStock <= p.ReorderLevel);

        var totalCount = await productsQuery.CountAsync();

        var items = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new StockLevelDto(p.Id, p.Sku, p.Name, p.CurrentStock, p.ReorderLevel, p.CurrentStock <= p.ReorderLevel))
            .ToListAsync();

        return new PagedResult<StockLevelDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<StockChangeResult> IssueStockAsync(IssueStockRequest request, long userId)
    {
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        return await ApplyStockChangeAsync(
            request.ProductId, -request.Quantity, TransactionType.ISSUED,
            request.Reason, request.ReferenceNo, userId);
    }

    public async Task<StockChangeResult> RecordDamagedOrLostAsync(DamagedLostRequest request, long userId)
    {
        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type) ||
            (type != TransactionType.DAMAGED && type != TransactionType.LOST))
            throw new InvalidOperationException("Type must be DAMAGED or LOST.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("A reason is required for damaged/lost stock.");

        return await ApplyStockChangeAsync(
            request.ProductId, -request.Quantity, type,
            request.Reason, null, userId);
    }

    public async Task<StockChangeResult> AdjustStockAsync(AdjustStockRequest request, long userId)
    {
        if (request.NewCount < 0)
            throw new InvalidOperationException("Stock count cannot be negative.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("A reason is required for stock adjustments.");

        var product = await _db.Products.FindAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        var quantityChange = request.NewCount - product.CurrentStock;

        return await ApplyStockChangeAsync(
            request.ProductId, quantityChange, TransactionType.ADJUSTMENT,
            request.Reason, null, userId, product);
    }

    public async Task<PagedResult<TransactionDto>> GetTransactionHistoryAsync(TransactionQueryParams query)
    {
        var txQuery = _db.InventoryTransactions.AsNoTracking().Include(t => t.Product).AsQueryable();

        if (query.ProductId.HasValue)
            txQuery = txQuery.Where(t => t.ProductId == query.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(query.Type) && Enum.TryParse<TransactionType>(query.Type, true, out var parsedType))
            txQuery = txQuery.Where(t => t.TransactionType == parsedType);

        if (query.FromDate.HasValue)
            txQuery = txQuery.Where(t => t.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            txQuery = txQuery.Where(t => t.CreatedAt <= query.ToDate.Value);

        var totalCount = await txQuery.CountAsync();

        var items = await txQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TransactionDto(
                t.Id, t.ProductId, t.Product!.Name, t.TransactionType.ToString(),
                t.QuantityChange, t.PreviousStock, t.NewStock, t.Reason, t.ReferenceNo,
                t.CreatedBy.ToString(), t.CreatedAt))
            .ToListAsync();

        return new PagedResult<TransactionDto>(items, totalCount, query.Page, query.PageSize);
    }

    private async Task<StockChangeResult> ApplyStockChangeAsync(
        long productId, int quantityChange, TransactionType type,
        string? reason, string? referenceNo, long userId, Product? preloadedProduct = null)
    {
        var product = preloadedProduct ?? await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        var previousStock = product.CurrentStock;
        var newStock = previousStock + quantityChange;

        if (newStock < 0)
            throw new InvalidOperationException($"Insufficient stock. Available: {previousStock}, requested: {-quantityChange}.");

        product.CurrentStock = newStock;

        var transaction = new InventoryTransaction
        {
            ProductId = productId,
            TransactionType = type,
            QuantityChange = quantityChange,
            PreviousStock = previousStock,
            NewStock = newStock,
            Reason = reason,
            ReferenceNo = referenceNo,
            CreatedBy = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.InventoryTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        return new StockChangeResult(productId, previousStock, newStock, quantityChange, transaction.Id);
    }
}