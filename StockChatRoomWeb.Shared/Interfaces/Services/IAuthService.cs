using StockChatRoomWeb.Shared.DTOs.Auth;

namespace StockChatRoomWeb.Shared.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(bool Succeeded, List<string>? Errors)> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(string userId, string username, string email);
}