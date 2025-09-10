using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockChatRoomWeb.Shared.DTOs.Stock;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Infrastructure.Services;

public class RabbitMQService : IMessageBrokerService, IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _stockRequestsQueue;
    private readonly string _stockResponsesQueue;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672/";
        _stockRequestsQueue = configuration["RabbitMQ:StockRequestsQueue"] ?? "stock.requests";
        _stockResponsesQueue = configuration["RabbitMQ:StockResponsesQueue"] ?? "stock.responses";

        var factory = new ConnectionFactory();
        factory.Uri = new Uri(connectionString);
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare queues
        _channel.QueueDeclare(queue: _stockRequestsQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: _stockResponsesQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task PublishStockRequestAsync(StockRequestMessage request)
    {
        try
        {
            var message = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: _stockRequestsQueue, basicProperties: properties, body: body);
            
            _logger.LogInformation("Published stock request for symbol: {Symbol}", request.StockSymbol);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing stock request for symbol: {Symbol}", request.StockSymbol);
            throw;
        }
    }

    public async Task PublishStockResponseAsync(StockResponseMessage response)
    {
        try
        {
            var message = JsonSerializer.Serialize(response);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: _stockResponsesQueue, basicProperties: properties, body: body);
            
            _logger.LogInformation("Published stock response for symbol: {Symbol}", response.StockSymbol);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing stock response for symbol: {Symbol}", response.StockSymbol);
            throw;
        }
    }

    public void StartConsumingStockResponses(Func<StockResponseMessage, Task> handler)
    {
        try
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var stockResponse = JsonSerializer.Deserialize<StockResponseMessage>(message);

                    if (stockResponse != null)
                    {
                        await handler(stockResponse);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize stock response message");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing stock response message");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _stockResponsesQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming stock responses from queue: {Queue}", _stockResponsesQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stock responses consumer");
            throw;
        }
    }

    public void StartConsumingStockRequests(Func<StockRequestMessage, Task> handler)
    {
        try
        {
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var stockRequest = JsonSerializer.Deserialize<StockRequestMessage>(message);

                    if (stockRequest != null)
                    {
                        await handler(stockRequest);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize stock request message");
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing stock request message");
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queue: _stockRequestsQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming stock requests from queue: {Queue}", _stockRequestsQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting stock requests consumer");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}