using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StockChatRoomWeb.Shared.DTOs.Stock;

namespace StooqBot;

public class StooqBotService : BackgroundService
{
    private readonly ILogger<StooqBotService> _logger;
    private readonly IConfiguration _configuration;
    private readonly StooqBot _stooqBot;
    private readonly StockDataParser _stockDataParser;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _stockRequestsQueue;
    private readonly string _stockResponsesQueue;

    public StooqBotService(ILogger<StooqBotService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _stooqBot = new StooqBot();
        _stockDataParser = new StockDataParser();

        var connectionString = _configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672/";
        _stockRequestsQueue = _configuration["RabbitMQ:StockRequestsQueue"] ?? "stock.requests";
        _stockResponsesQueue = _configuration["RabbitMQ:StockResponsesQueue"] ?? "stock.responses";

        var factory = new ConnectionFactory();
        factory.Uri = new Uri(connectionString);
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare queues
        _channel.QueueDeclare(queue: _stockRequestsQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: _stockResponsesQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        _logger.LogInformation("StooqBotService initialized with RabbitMQ connection");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StooqBotService started - listening for stock requests");

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Received stock request: {Message}", message);
                
                var stockRequest = JsonSerializer.Deserialize<StockRequestMessage>(message);
                
                if (stockRequest != null)
                {
                    await ProcessStockRequestAsync(stockRequest);
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
                _logger.LogError(ex, "Error processing stock request");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queue: _stockRequestsQueue, autoAck: false, consumer: consumer);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("StooqBotService stopped");
    }

    private async Task ProcessStockRequestAsync(StockRequestMessage request)
    {
        try
        {
            _logger.LogInformation("Processing stock request for symbol: {Symbol}", request.StockSymbol);

            // Use existing StooqBot logic
            var rawData = await _stooqBot.ExecuteRequest(request.StockSymbol);

            StockResponseMessage response;

            if (_stockDataParser.IsValidStockData(rawData))
            {
                // Parse CSV data to extract price
                var price = _stockDataParser.ParsePriceFromCsv(rawData);
                var formattedMessage = $"The quote for {request.StockSymbol.ToUpperInvariant()} is ${price:F2} per share.";

                response = new StockResponseMessage
                {
                    RequestId = request.RequestId,
                    StockSymbol = request.StockSymbol,
                    Price = price,
                    FormattedMessage = formattedMessage,
                    Timestamp = DateTime.UtcNow,
                    IsSuccess = true
                };

                _logger.LogInformation("Successfully processed stock request for {Symbol}: ${Price:F2}", request.StockSymbol, price);
            }
            else
            {
                // Handle invalid/not found stock data
                response = new StockResponseMessage
                {
                    RequestId = request.RequestId,
                    StockSymbol = request.StockSymbol,
                    Price = null,
                    FormattedMessage = $"Sorry, I couldn't find stock information for {request.StockSymbol.ToUpperInvariant()}.",
                    Timestamp = DateTime.UtcNow,
                    IsSuccess = false,
                    ErrorMessage = "Stock data not found or invalid"
                };

                _logger.LogWarning("Stock data not found for symbol: {Symbol}", request.StockSymbol);
            }

            // Publish response back to RabbitMQ
            await PublishStockResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stock request for symbol: {Symbol}", request.StockSymbol);

            // Send error response
            var errorResponse = new StockResponseMessage
            {
                RequestId = request.RequestId,
                StockSymbol = request.StockSymbol,
                Price = null,
                FormattedMessage = $"Sorry, there was an error retrieving stock information for {request.StockSymbol.ToUpperInvariant()}.",
                Timestamp = DateTime.UtcNow,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };

            await PublishStockResponseAsync(errorResponse);
        }
    }


    private async Task PublishStockResponseAsync(StockResponseMessage response)
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

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}