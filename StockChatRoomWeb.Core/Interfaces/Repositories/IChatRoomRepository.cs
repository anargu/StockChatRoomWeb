using StockChatRoomWeb.Core.Entities;

namespace StockChatRoomWeb.Core.Interfaces.Repositories;

public interface IChatRoomRepository
{
    Task<ChatRoom?> GetByIdAsync(Guid id);
    Task<IEnumerable<ChatRoom>> GetAllAsync();
    Task<IEnumerable<ChatRoom>> GetByUserIdAsync(Guid userId);
    Task<ChatRoom> CreateAsync(ChatRoom chatRoom);
    Task<ChatRoom> UpdateAsync(ChatRoom chatRoom);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // INFO: For Demo Purpose Only Listing recent chat rooms is limited.
    Task<IEnumerable<ChatRoom>> GetRecentRoomsAsync(int count = 20);
}