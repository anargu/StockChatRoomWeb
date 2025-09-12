using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.ChatRoom;

namespace StockChatRoomWeb.Shared.Interfaces.Services;

public interface IChatService
{
    Task<List<ChatMessageDto>> GetRecentMessagesAsync(int count = 50);
    Task<ChatMessageDto> SendMessageAsync(string userId, string content);
    Task<ChatMessageDto> CreateStockCommandDisplayAsync(string userId, string content);
    Task<bool> IsStockCommandAsync(string content);
    Task<string> ExtractStockSymbolAsync(string content);
    Task<List<ChatMessageDto>> GetRecentMessagesAsync(Guid? chatRoomId, int count = 50);
    Task<ChatMessageDto> SendMessageAsync(string userId, string content, Guid? chatRoomId);
    Task<List<ChatRoomDto>> GetAllChatRoomsAsync();
    Task<ChatRoomDto> CreateChatRoomAsync(string userId, string name);
    Task<ChatRoomDto?> GetChatRoomAsync(Guid chatRoomId);
}