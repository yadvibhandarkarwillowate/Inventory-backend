namespace InventoryApp.Application.DTOs.PurchaseOrder;

// --- Supplier DTOs ---
public record CreateSupplierDto(
    string Name,
    string? ContactPerson = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null
);

public record SupplierDto(
    long Id,
    string Name,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    DateTimeOffset CreatedAt
);

// --- Purchase Order DTOs ---
public record CreatePurchaseOrderItemDto(
    long ProductId,
    int OrderedQty,
    decimal PurchasePrice
);

public record CreatePurchaseOrderDto(
    long SupplierId,
    DateTimeOffset? OrderDate,
    List<CreatePurchaseOrderItemDto> Items
);

public record PurchaseOrderItemDto(
    long Id,
    long ProductId,
    string ProductName,
    string ProductSku,
    int OrderedQty,
    int ReceivedQty,
    decimal PurchasePrice,
    decimal Total
);

public record PurchaseOrderResponseDto(
    long Id,
    string OrderNumber,
    long SupplierId,
    string SupplierName,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount,
    long CreatedBy,
    DateTimeOffset CreatedAt,
    List<PurchaseOrderItemDto> Items
);

public record PurchaseOrderListDto(
    long Id,
    string OrderNumber,
    long SupplierId,
    string SupplierName,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount,
    int TotalItems,
    DateTimeOffset CreatedAt
);

public record PurchaseOrderQueryParams(
    string? Search = null,
    string? Status = null,
    long? SupplierId = null,
    int Page = 1,
    int PageSize = 20
);

// --- Stock Receiving DTOs ---
public record ReceiveStockItemDto(
    long PurchaseOrderItemId,
    int QuantityReceived
);

public record ReceiveStockRequestDto(
    List<ReceiveStockItemDto> Items
);

public record ReceiveStockResultDto(
    long PurchaseOrderId,
    string OrderNumber,
    string Status,
    int TotalItemsReceived
);
