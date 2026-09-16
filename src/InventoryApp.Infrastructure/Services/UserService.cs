using InventoryApp.Application.DTOs.user;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace InventoryApp.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher = new();

    private readonly IConfiguration _configuration;

    public UserService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        var existingUser = await _db.Users
            .FirstOrDefaultAsync(u => u.email == dto.Email);

        if (existingUser != null)
            throw new InvalidOperationException("Email is already registered.");

        var hashedPassword = _passwordHasher.HashPassword(null!, dto.Password);
        
        // create user 
        var user = new User
        {
            name = dto.Name,
            email = dto.Email,
            password = hashedPassword,
            role = dto.Role,
            status = dto.Status
        };
        
        // save user 
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.id,
            Name = user.name,
            Email = user.email,
            Role = user.role,
            Status = user.status
        };

        // We'll implement this next
        // throw new NotImplementedException();
    }


// login methods 
    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.email == dto.Email);

        if (user == null)
            return null;

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.password,
            dto.Password
        );

        if (passwordResult == PasswordVerificationResult.Failed)
            return null;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
            new Claim(ClaimTypes.Name, user.name),
            new Claim(ClaimTypes.Email, user.email),
            new Claim(ClaimTypes.Role, user.role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!
            )
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expires = DateTime.UtcNow.AddMinutes(
            double.Parse(
                _configuration["Jwt:ExpiresInMinutes"] ?? "60"
            )
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),

            User = new UserResponseDto
            {
                Id = user.id,
                Name = user.name,
                Email = user.email,
                Role = user.role,
                Status = user.status
            }
        };
    }
}
