using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Core.Enums;
using StockChatRoomWeb.Core.Interfaces.Repositories;
using StockChatRoomWeb.Core.Services;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public ChatService(IChatMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<List<ChatMessageDto>> GetRecentMessagesAsync(int count = 50)
    {
        var messages = await _messageRepository.GetRecentMessagesAsync(count);
        
        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            Content = m.Content,
            Username = m.User.UserName ?? "Unknown",
            UserId = m.UserId,
            IsFromBot = m.IsFromBot,
            MessageType = (StockChatRoomWeb.Shared.DTOs.Chat.MessageType)m.MessageType,
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<ChatMessageDto> SendMessageAsync(string userId, string content)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userGuid);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Only save regular messages to database (NOT stock commands)
        var message = new ChatMessage
        {
            Content = content.Trim(),
            UserId = userGuid,
            IsFromBot = false,
            MessageType = StockChatRoomWeb.Core.Enums.MessageType.Normal,
            User = user
        };

        var savedMessage = await _messageRepository.CreateAsync(message);

        return new ChatMessageDto
        {
            Id = savedMessage.Id,
            Content = savedMessage.Content,
            Username = savedMessage.User.UserName ?? "Unknown",
            UserId = savedMessage.UserId,
            IsFromBot = savedMessage.IsFromBot,
            MessageType = (StockChatRoomWeb.Shared.DTOs.Chat.MessageType)savedMessage.MessageType,
            CreatedAt = savedMessage.CreatedAt
        };
    }

    public async Task<ChatMessageDto> CreateStockCommandDisplayAsync(string userId, string content)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userGuid);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Create display DTO for stock command WITHOUT saving to database
        return new ChatMessageDto
        {
            Id = Guid.NewGuid(), // Temporary ID for display only
            Content = content.Trim(),
            Username = user.UserName ?? "Unknown",
            UserId = userGuid,
            IsFromBot = false,
            MessageType = StockChatRoomWeb.Shared.DTOs.Chat.MessageType.StockCommand,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<bool> IsStockCommandAsync(string content)
    {
        await Task.CompletedTask; // Async for future extensibility
        return StockCommandParser.IsStockCommand(content);
    }

    public async Task<string> ExtractStockSymbolAsync(string content)
    {
        await Task.CompletedTask; // Async for future extensibility
        return StockCommandParser.ExtractStockSymbol(content) ?? string.Empty;
    }
}