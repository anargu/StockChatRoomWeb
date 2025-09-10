using StockChatRoomWeb.Shared.DTOs.Chat;

namespace StockChatRoomWeb.Shared.Interfaces.Services;

public interface IChatService
{
    Task<List<ChatMessageDto>> GetRecentMessagesAsync(int count = 50);
    Task<ChatMessageDto> SendMessageAsync(string userId, string content);
    Task<ChatMessageDto> CreateStockCommandDisplayAsync(string userId, string content);
    Task<bool> IsStockCommandAsync(string content);
    Task<string> ExtractStockSymbolAsync(string content);
}