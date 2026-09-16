using InventoryApp.Application.DTOs.Products;

namespace InventoryApp.Application.Interfaces;

public interface IProductService
{
    Task<ProductPagedResult> GetProductsAsync(ProductQueryParams query);
    Task<ProductDetailDto?> GetProductByIdAsync(long id);
    Task<ProductDetailDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDetailDto> UpdateProductAsync(long id, UpdateProductRequest request);
    Task DeleteProductAsync(long id);
    Task<List<LookupDto>> GetCategoryLookupAsync();
    Task<List<LookupDto>> GetSupplierLookupAsync();
}