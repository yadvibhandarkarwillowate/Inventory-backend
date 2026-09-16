using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.DTOs.PurchaseOrder;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Domain.Enums;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Application.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly AppDbContext _db;

    public PurchaseOrderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PurchaseOrderResponseDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, long userId)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("Purchase order must contain at least one item.");

        var supplier = await _db.Suppliers.FindAsync(dto.SupplierId)
            ?? throw new KeyNotFoundException($"Supplier with ID {dto.SupplierId} not found.");

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var productId in productIds)
        {
            if (!products.ContainsKey(productId))
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
        }

        var poItems = new List<PurchaseOrderItem>();
        decimal totalAmount = 0;

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.OrderedQty <= 0)
                throw new InvalidOperationException($"Ordered quantity must be greater than zero for product ID {itemDto.ProductId}.");

            if (itemDto.PurchasePrice < 0)
                throw new InvalidOperationException($"Purchase price cannot be negative for product ID {itemDto.ProductId}.");

            var lineTotal = itemDto.OrderedQty * itemDto.PurchasePrice;
            totalAmount += lineTotal;

            poItems.Add(new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                OrderedQty = itemDto.OrderedQty,
                ReceivedQty = 0,
                PurchasePrice = itemDto.PurchasePrice,
                Total = lineTotal
            });
        }

        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..4].ToUpper()}";

        var po = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = dto.SupplierId,
            OrderDate = dto.OrderDate ?? DateTimeOffset.UtcNow,
            Status = PurchaseOrderStatus.PENDING,
            TotalAmount = totalAmount,
            CreatedBy = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = poItems
        };

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        return MapToResponseDto(po, supplier.Name, products);
    }

    public async Task<PagedResult<PurchaseOrderListDto>> GetPurchaseOrdersAsync(PurchaseOrderQueryParams query)
    {
        var poQuery = _db.PurchaseOrders.AsNoTracking().Include(p => p.Supplier).Include(p => p.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            poQuery = poQuery.Where(p =>
                p.OrderNumber.ToLower().Contains(term) ||
                (p.Supplier != null && p.Supplier.Name.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<PurchaseOrderStatus>(query.Status, true, out var parsedStatus))
        {
            poQuery = poQuery.Where(p => p.Status == parsedStatus);
        }

        if (query.SupplierId.HasValue)
        {
            poQuery = poQuery.Where(p => p.SupplierId == query.SupplierId.Value);
        }

        var totalCount = await poQuery.CountAsync();

        var items = await poQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new PurchaseOrderListDto(
                p.Id,
                p.OrderNumber,
                p.SupplierId,
                p.Supplier != null ? p.Supplier.Name : string.Empty,
                p.OrderDate,
                p.Status.ToString(),
                p.TotalAmount,
                p.Items.Count,
                p.CreatedAt
            ))
            .ToListAsync();

        return new PagedResult<PurchaseOrderListDto>(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<PurchaseOrderResponseDto?> GetPurchaseOrderByIdAsync(long id)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (po == null) return null;

        var itemDtos = po.Items.Select(i => new PurchaseOrderItemDto(
            i.Id,
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.Sku ?? string.Empty,
            i.OrderedQty,
            i.ReceivedQty,
            i.PurchasePrice,
            i.Total
        )).ToList();

        return new PurchaseOrderResponseDto(
            po.Id,
            po.OrderNumber,
            po.SupplierId,
            po.Supplier?.Name ?? string.Empty,
            po.OrderDate,
            po.Status.ToString(),
            po.TotalAmount,
            po.CreatedBy,
            po.CreatedAt,
            itemDtos
        );
    }

    public async Task<ReceiveStockResultDto> ReceiveStockAsync(long poId, ReceiveStockRequestDto request, long userId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("Stock receiving request must contain at least one item.");

        var po = await _db.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == poId)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {poId} not found.");

        if (po.Status == PurchaseOrderStatus.CANCELLED)
            throw new InvalidOperationException("Cannot receive stock for a cancelled Purchase Order.");

        if (po.Status == PurchaseOrderStatus.RECEIVED)
            throw new InvalidOperationException("This Purchase Order has already been fully received.");

        using var transaction = await _db.Database.BeginTransactionAsync();

        int totalItemsReceivedCount = 0;

        foreach (var receiveItem in request.Items)
        {
            if (receiveItem.QuantityReceived <= 0)
                throw new InvalidOperationException("Quantity received must be greater than zero.");

            var poItem = po.Items.FirstOrDefault(i => i.Id == receiveItem.PurchaseOrderItemId)
                ?? throw new InvalidOperationException($"Purchase Order Item with ID {receiveItem.PurchaseOrderItemId} does not belong to Purchase Order {po.OrderNumber}.");

            int remainingQty = poItem.OrderedQty - poItem.ReceivedQty;
            if (receiveItem.QuantityReceived > remainingQty)
            {
                throw new InvalidOperationException(
                    $"Received quantity ({receiveItem.QuantityReceived}) exceeds remaining quantity ({remainingQty}) for PO Item ID {poItem.Id}.");
            }

            var product = await _db.Products.FindAsync(poItem.ProductId)
                ?? throw new KeyNotFoundException($"Product with ID {poItem.ProductId} not found.");

            int previousStock = product.CurrentStock;
            int newStock = previousStock + receiveItem.QuantityReceived;

            // 1. Update Product CurrentStock
            product.CurrentStock = newStock;

            // 2. Update PO Item ReceivedQty
            poItem.ReceivedQty += receiveItem.QuantityReceived;

            // 3. Create ONE InventoryTransaction (TransactionType = RECEIVED)
            var inventoryTx = new InventoryTransaction
            {
                ProductId = product.Id,
                TransactionType = TransactionType.RECEIVED,
                QuantityChange = receiveItem.QuantityReceived,
                PreviousStock = previousStock,
                NewStock = newStock,
                Reason = $"Stock received for Purchase Order {po.OrderNumber}",
                ReferenceNo = po.OrderNumber,
                CreatedBy = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.InventoryTransactions.Add(inventoryTx);
            totalItemsReceivedCount += receiveItem.QuantityReceived;
        }

        // 4. Update PO status
        bool allFullyReceived = po.Items.All(i => i.ReceivedQty >= i.OrderedQty);
        bool anyReceived = po.Items.Any(i => i.ReceivedQty > 0);

        if (allFullyReceived)
        {
            po.Status = PurchaseOrderStatus.RECEIVED;
        }
        else if (anyReceived)
        {
            po.Status = PurchaseOrderStatus.PARTIALLY_RECEIVED;
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ReceiveStockResultDto(po.Id, po.OrderNumber, po.Status.ToString(), totalItemsReceivedCount);
    }

    private static PurchaseOrderResponseDto MapToResponseDto(
        PurchaseOrder po, string supplierName, Dictionary<long, Product> products)
    {
        var itemDtos = po.Items.Select(i => new PurchaseOrderItemDto(
            i.Id,
            i.ProductId,
            products.TryGetValue(i.ProductId, out var p) ? p.Name : string.Empty,
            products.TryGetValue(i.ProductId, out p) ? p.Sku : string.Empty,
            i.OrderedQty,
            i.ReceivedQty,
            i.PurchasePrice,
            i.Total
        )).ToList();

        return new PurchaseOrderResponseDto(
            po.Id,
            po.OrderNumber,
            po.SupplierId,
            supplierName,
            po.OrderDate,
            po.Status.ToString(),
            po.TotalAmount,
            po.CreatedBy,
            po.CreatedAt,
            itemDtos
        );
    }
}
