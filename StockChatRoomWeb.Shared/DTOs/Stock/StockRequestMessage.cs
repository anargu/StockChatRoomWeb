namespace StockChatRoomWeb.Shared.DTOs.Stock;

public class StockRequestMessage
{
    public string StockSymbol { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; }
}