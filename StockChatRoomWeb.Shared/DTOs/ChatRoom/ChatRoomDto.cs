namespace StockChatRoomWeb.Shared.DTOs.ChatRoom;

public class ChatRoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Chat Room creator
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}