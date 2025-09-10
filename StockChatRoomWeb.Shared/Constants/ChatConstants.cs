namespace StockChatRoomWeb.Shared.Constants;

public static class ChatConstants
{
    public const string StockCommandPrefix = "/stock=";
    public const string ChatRoomName = "ChatRoom";
    public const string BotUsername = "StockBot";
    public const int MaxMessagesCount = 50;
    
    public static class RabbitMQ
    {
        public const string StockRequestsQueue = "stock.requests";
        public const string StockResponsesQueue = "stock.responses";
        public const string ChatExchange = "chat.exchange";
        public const string StockRequestRoutingKey = "stock.request";
        public const string StockResponseRoutingKey = "stock.response";
    }
}