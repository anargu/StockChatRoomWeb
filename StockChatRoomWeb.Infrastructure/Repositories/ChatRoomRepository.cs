using Microsoft.EntityFrameworkCore;
using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Core.Interfaces.Repositories;
using StockChatRoomWeb.Infrastructure.Data;

namespace StockChatRoomWeb.Infrastructure.Repositories;

public class ChatRoomRepository : IChatRoomRepository
{
    private readonly ApplicationDbContext _context;

    public ChatRoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom?> GetByIdAsync(Guid id)
    {
        return await _context.ChatRooms
            .Include(cr => cr.User)
            .Include(cr => cr.ChatMessages.OrderByDescending(m => m.CreatedAt).Take(50))
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<IEnumerable<ChatRoom>> GetAllAsync()
    {
        return await _context.ChatRooms
            .Include(cr => cr.User)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ChatRoom>> GetByUserIdAsync(Guid userId)
    {
        return await _context.ChatRooms
            .Include(cr => cr.User)
            .Where(cr => cr.UserId == userId)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatRoom> CreateAsync(ChatRoom chatRoom)
    {
        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task<ChatRoom> UpdateAsync(ChatRoom chatRoom)
    {
        _context.Entry(chatRoom).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return chatRoom;
    }

    public async Task DeleteAsync(Guid id)
    {
        var chatRoom = await _context.ChatRooms.FindAsync(id);
        if (chatRoom != null)
        {
            _context.ChatRooms.Remove(chatRoom);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ChatRooms.AnyAsync(cr => cr.Id == id);
    }

    public async Task<IEnumerable<ChatRoom>> GetRecentRoomsAsync(int count = 20)
    {
        return await _context.ChatRooms
            .Include(cr => cr.User)
            .OrderByDescending(cr => cr.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}