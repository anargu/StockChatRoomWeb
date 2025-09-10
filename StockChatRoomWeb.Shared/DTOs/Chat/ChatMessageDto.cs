namespace StockChatRoomWeb.Shared.DTOs.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public bool IsFromBot { get; set; }
    public MessageType MessageType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum MessageType
{
    Normal = 0,

    // This type should not be used.
    StockCommand = 1,
    StockResponse = 2
}