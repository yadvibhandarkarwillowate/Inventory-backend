using InventoryApp.Application.DTOs.Products;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<ProductPagedResult> GetProductsAsync(ProductQueryParams query)
    {
        var q = _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(term) || p.Sku.ToLower().Contains(term));
        }

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.SupplierId.HasValue)
            q = q.Where(p => p.SupplierId == query.SupplierId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(p => p.Status == query.Status);

        var totalCount = await q.CountAsync();

        var items = await q
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductListDto(
                p.Id, p.Name, p.Brand, p.Sku,
                p.Category != null ? p.Category.Name : null,
                p.Supplier != null ? p.Supplier.Name : null,
                p.SellingPrice, p.CurrentStock, p.Status))
            .ToListAsync();

        return new ProductPagedResult(items, totalCount, query.Page, query.PageSize);
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(long id)
    {
        var p = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == id);

        return p == null ? null : ToDetailDto(p);
    }

    public async Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request)
    {
        var skuExists = await _db.Products.AnyAsync(p => p.Sku == request.Sku);
        if (skuExists)
            throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists.");

        if (request.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new InvalidOperationException("Selected category does not exist.");

        if (request.SupplierId.HasValue && !await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId))
            throw new InvalidOperationException("Selected supplier does not exist.");

        var product = new Product
        {
            Name = request.Name,
            Brand = request.Brand,
            ModelNumber = request.ModelNumber,
            Sku = request.Sku,
            Barcode = request.Barcode,
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            CurrentStock = 0,
            ReorderLevel = request.ReorderLevel,
            WarrantyPeriod = request.WarrantyPeriod,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            Status = "ACTIVE"
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductDetailDto> UpdateProductAsync(long id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        if (request.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            throw new InvalidOperationException("Selected category does not exist.");

        if (request.SupplierId.HasValue && !await _db.Suppliers.AnyAsync(s => s.Id == request.SupplierId))
            throw new InvalidOperationException("Selected supplier does not exist.");

        product.Name = request.Name;
        product.Brand = request.Brand;
        product.ModelNumber = request.ModelNumber;
        product.Barcode = request.Barcode;
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.PurchasePrice = request.PurchasePrice;
        product.SellingPrice = request.SellingPrice;
        product.ReorderLevel = request.ReorderLevel;
        product.WarrantyPeriod = request.WarrantyPeriod;
        product.Description = request.Description;
        product.ImageUrl = request.ImageUrl;
        product.Status = request.Status;

        await _db.SaveChangesAsync();

        return (await GetProductByIdAsync(id))!;
    }

    public async Task DeleteProductAsync(long id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");

        var hasTransactions = await _db.InventoryTransactions.AnyAsync(t => t.ProductId == id);
        if (hasTransactions)
            throw new InvalidOperationException(
                "Cannot delete a product with existing stock movement history. Consider marking it Inactive instead.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }

    public async Task<List<LookupDto>> GetCategoryLookupAsync() =>
        await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new LookupDto(c.Id, c.Name))
            .ToListAsync();

    public async Task<List<LookupDto>> GetSupplierLookupAsync() =>
        await _db.Suppliers.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new LookupDto(s.Id, s.Name))
            .ToListAsync();

    private static ProductDetailDto ToDetailDto(Product p) => new(
        p.Id, p.Name, p.Brand, p.ModelNumber, p.Sku, p.Barcode,
        p.CategoryId, p.Category?.Name, p.SupplierId, p.Supplier?.Name,
        p.PurchasePrice, p.SellingPrice, p.CurrentStock, p.ReorderLevel,
        p.WarrantyPeriod, p.Description, p.ImageUrl, p.Status);
}
