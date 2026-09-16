using InventoryApp.Application.DTOs.user ; 
namespace InventoryApp.Application.Interfaces ; 

public interface IUserService 
{   
    // "Any class implementing IUserService must provide a method called CreateUserAsync 
    // that accepts UserCreateDto and eventually returns a UserResponseDto."

    // returun            provide these servide    accept these data 
    Task<UserResponseDto> CreateUserAsync( UserCreateDto dto ) ; 
    Task<LoginResponseDto?> LoginAsync(LoginDto dto )  ;  
}