using System.ComponentModel.DataAnnotations;

namespace StockChatRoomWeb.Core.Entities;

public class ChatRoom
{
    public ChatRoom()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ChatMessages = new List<ChatMessage>();
    }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid UserId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ChatMessage>  ChatMessages { get; set; }
}