using Microsoft.AspNetCore.SignalR;
using StockChatRoomWeb.Api.Hubs;
using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Core.Interfaces.Repositories;
using StockChatRoomWeb.Shared.Constants;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.Stock;
using StockChatRoomWeb.Shared.Interfaces.Services;
using StockChatRoomWeb.Shared.Utils;

namespace StockChatRoomWeb.Api.Services;

public class StockResponseBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<StockResponseBackgroundService> _logger;

    public StockResponseBackgroundService(
        IServiceProvider serviceProvider,
        IMessageBrokerService messageBrokerService,
        IHubContext<ChatHub> hubContext,
        ILogger<StockResponseBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _messageBrokerService = messageBrokerService;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Response Background Service started");

        _messageBrokerService.StartConsumingStockResponses(HandleStockResponse);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Stock Response Background Service stopped");
    }

    private async Task HandleStockResponse(StockResponseMessage response)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var messageRepository = scope.ServiceProvider.GetRequiredService<IChatMessageRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Create bot user if not exists
            var botUser = await GetOrCreateBotUserAsync(userRepository);

            // Create chat message from bot
            var botMessage = new ChatMessage
            {
                Content = response.FormattedMessage,
                UserId = botUser.Id,
                IsFromBot = true,
                MessageType = StockChatRoomWeb.Core.Enums.MessageType.StockResponse,
                User = botUser,
                ChatRoomId = response.ChatRoomId,
            };

            var savedMessage = await messageRepository.CreateAsync(botMessage);

            // Create DTO for SignalR
            var messageDto = new ChatMessageDto
            {
                Id = savedMessage.Id,
                Content = savedMessage.Content,
                Username = ChatConstants.BotUsername,
                UserId = savedMessage.UserId,
                IsFromBot = savedMessage.IsFromBot,
                MessageType = StockChatRoomWeb.Shared.DTOs.Chat.MessageType.StockResponse,
                CreatedAt = savedMessage.CreatedAt,
                ChatRoomId = savedMessage.ChatRoomId
            };

            if (response.ChatRoomId.HasValue)
            {
                var roomGroupName = ChatUtils.GetRoomGroupName(response.ChatRoomId.ToString());
                // Broadcast to specific chat room
                await _hubContext.Clients.Group(roomGroupName).SendAsync("ReceiveMessage", messageDto);
            }
            else
            {
                // Broadcast to global chat
                await _hubContext.Clients.Group(ChatConstants.ChatRoomName).SendAsync("ReceiveMessage", messageDto);
            }   
            // Broadcast to all connected clients
            await _hubContext.Clients.Group(ChatConstants.ChatRoomName).SendAsync("ReceiveMessage", messageDto);

            _logger.LogInformation("Processed stock response for symbol: {Symbol}", response.StockSymbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stock response for symbol: {Symbol}", response.StockSymbol);
        }
    }

    private async Task<User> GetOrCreateBotUserAsync(IUserRepository userRepository)
    {
        var botUser = await userRepository.GetByUsernameAsync(ChatConstants.BotUsername);
        
        if (botUser == null)
        {
            botUser = new User
            {
                UserName = ChatConstants.BotUsername,
                Email = "bot@stockchat.com",
                EmailConfirmed = true
            };

            botUser = await userRepository.CreateAsync(botUser);
            _logger.LogInformation("Created bot user: {Username}", ChatConstants.BotUsername);
        }

        return botUser;
    }
}