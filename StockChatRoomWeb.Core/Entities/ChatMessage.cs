using System.ComponentModel.DataAnnotations;
using StockChatRoomWeb.Core.Enums;

namespace StockChatRoomWeb.Core.Entities;

public class ChatMessage
{
    public ChatMessage()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public bool IsFromBot { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Normal;

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}