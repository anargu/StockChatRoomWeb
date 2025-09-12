using StockChatRoomWeb.Core.Entities;

namespace StockChatRoomWeb.Core.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<ChatMessage?> GetByIdAsync(Guid id);
    Task<List<ChatMessage>> GetRecentMessagesAsync(Guid? chatRoomId, int count = 50);
    Task<List<ChatMessage>> GetMessagesByUserAsync(Guid userId);
    Task<ChatMessage> CreateAsync(ChatMessage message);
    Task<ChatMessage> UpdateAsync(ChatMessage message);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetTotalMessagesCountAsync();
}