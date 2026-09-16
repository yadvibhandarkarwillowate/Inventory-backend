namespace InventoryApp.Application.DTOs.user  ; 

public class UserCreateDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
    public required string Status { get; set; }
}