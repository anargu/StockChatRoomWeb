using Microsoft.AspNetCore.Identity;

namespace StockChatRoomWeb.Core.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ChatMessages = new HashSet<ChatMessage>();
    }

    public DateTime CreatedAt { get; set; }
    public virtual ICollection<ChatMessage> ChatMessages { get; set; }
}