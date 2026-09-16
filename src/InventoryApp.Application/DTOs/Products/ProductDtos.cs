namespace InventoryApp.Application.DTOs.Products;

public record ProductListDto(
    long Id, string Name, string? Brand, string Sku,
    string? CategoryName, string? SupplierName,
    decimal SellingPrice, int CurrentStock, string Status
);

public record ProductDetailDto(
    long Id, string Name, string? Brand, string? ModelNumber,
    string Sku, string? Barcode, long? CategoryId, string? CategoryName,
    long? SupplierId, string? SupplierName,
    decimal PurchasePrice, decimal SellingPrice,
    int CurrentStock, int ReorderLevel, string? WarrantyPeriod,
    string? Description, string? ImageUrl, string Status
);

public record ProductPagedResult(List<ProductListDto> Items, int TotalCount, int Page, int PageSize);

public record ProductQueryParams(
    string? Search = null, long? CategoryId = null, long? SupplierId = null,
    string? Status = null, int Page = 1, int PageSize = 20
);

public record CreateProductRequest(
    string Name, string? Brand, string? ModelNumber, string Sku, string? Barcode,
    long? CategoryId, long? SupplierId, decimal PurchasePrice, decimal SellingPrice,
    int ReorderLevel, string? WarrantyPeriod, string? Description, string? ImageUrl
);

public record UpdateProductRequest(
    string Name, string? Brand, string? ModelNumber, string? Barcode,
    long? CategoryId, long? SupplierId, decimal PurchasePrice, decimal SellingPrice,
    int ReorderLevel, string? WarrantyPeriod, string? Description, string? ImageUrl, string Status
);

public record LookupDto(long Id, string Name);