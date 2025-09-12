using System.ComponentModel.DataAnnotations;

namespace StockChatRoomWeb.Shared.DTOs.ChatRoom;

public class CreateChatRoomRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}