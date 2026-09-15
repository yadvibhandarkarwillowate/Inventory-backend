namespace InventoryApp.Application.DTOs.user;

public class LoginResponseDto
{
    // sending token and the user response data after login 
    public required string Token { get; set; }
    public required UserResponseDto User { get; set; }
}