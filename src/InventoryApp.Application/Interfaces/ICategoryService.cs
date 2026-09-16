using InventoryApp.Application.DTOs.Category;

namespace InventoryApp.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllAsync();

    Task<CategoryResponseDto?> GetByIdAsync(long id);

    Task<CategoryResponseDto> CreateAsync(CreateCategoryDto dto);

    Task<bool> UpdateAsync(long id, UpdateCategoryDto dto);

    Task<bool> DeleteAsync(long id);
}