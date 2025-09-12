using Microsoft.AspNetCore.Identity;

namespace StockChatRoomWeb.Core.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ChatMessages = new HashSet<ChatMessage>();
        ChatRooms = new List<ChatRoom>();
    }

    public DateTime CreatedAt { get; set; }
    public virtual ICollection<ChatMessage> ChatMessages { get; set; }
    public virtual ICollection<ChatRoom> ChatRooms { get; set; }
}