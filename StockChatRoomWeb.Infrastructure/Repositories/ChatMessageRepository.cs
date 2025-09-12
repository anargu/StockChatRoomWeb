using Microsoft.EntityFrameworkCore;
using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Core.Interfaces.Repositories;
using StockChatRoomWeb.Infrastructure.Data;

namespace StockChatRoomWeb.Infrastructure.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly ApplicationDbContext _context;

    public ChatMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatMessage?> GetByIdAsync(Guid id)
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<ChatMessage>> GetRecentMessagesAsync(int count = 50)
    {
        return await GetRecentMessagesAsync(null, count); // Global chat
    }

    public async Task<List<ChatMessage>> GetRecentMessagesAsync(Guid? chatRoomId, int count = 50)
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesByUserAsync(Guid userId)
    {
        return await _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Load the user information
        await _context.Entry(message)
            .Reference(m => m.User)
            .LoadAsync();

        return message;
    }

    public async Task<ChatMessage> UpdateAsync(ChatMessage message)
    {
        _context.ChatMessages.Update(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var message = await _context.ChatMessages.FindAsync(id);
        if (message == null)
            return false;

        _context.ChatMessages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTotalMessagesCountAsync()
    {
        return await _context.ChatMessages.CountAsync();
    }
}