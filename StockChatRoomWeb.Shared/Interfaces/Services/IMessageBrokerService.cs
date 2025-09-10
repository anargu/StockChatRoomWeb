using StockChatRoomWeb.Shared.DTOs.Stock;

namespace StockChatRoomWeb.Shared.Interfaces.Services;

public interface IMessageBrokerService
{
    Task PublishStockRequestAsync(StockRequestMessage request);
    Task PublishStockResponseAsync(StockResponseMessage response);
    void StartConsumingStockResponses(Func<StockResponseMessage, Task> handler);
    void StartConsumingStockRequests(Func<StockRequestMessage, Task> handler);
}