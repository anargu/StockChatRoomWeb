using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Shared.DTOs.Auth;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return null;

        var token = GenerateJwtToken(user.Id.ToString(), user.UserName!, user.Email!);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new LoginResponse
        {
            Token = token,
            Username = user.UserName!,
            Email = user.Email!,
            ExpiresAt = expiresAt
        };
    }

    public async Task<(bool Succeeded, List<string>? Errors)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return (false, new List<string> { "Email is already in use." });

        var existingUserByUsername = await _userManager.FindByNameAsync(request.Username);
        if (existingUserByUsername != null)
            return (false, new List<string> { "Username is already in use." });

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var listErrors = result.Errors.ToList();

            return (false, listErrors.Select(e => e.Description).ToList());
        }
        
        return (result.Succeeded, null);
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = await tokenHandler.ValidateTokenAsync(token, validationParameters);
            return principal.IsValid;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateJwtToken(string userId, string username, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim("UserId", userId),
                new Claim("Username", username)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}