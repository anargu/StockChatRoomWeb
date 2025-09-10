namespace StockChatRoomWeb.Shared.DTOs.Stock;

public class StockResponseMessage
{
    public Guid RequestId { get; set; }
    public string StockSymbol { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string FormattedMessage { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}