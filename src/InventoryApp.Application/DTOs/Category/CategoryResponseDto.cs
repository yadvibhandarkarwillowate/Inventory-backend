namespace InventoryApp.Application.DTOs.Category;

public class CategoryResponseDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
