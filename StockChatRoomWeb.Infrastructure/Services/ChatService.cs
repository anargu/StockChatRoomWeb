using StockChatRoomWeb.Core.Entities;
using StockChatRoomWeb.Core.Enums;
using StockChatRoomWeb.Core.Interfaces.Repositories;
using StockChatRoomWeb.Core.Services;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.ChatRoom;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;

    public ChatService(IChatMessageRepository messageRepository, IUserRepository userRepository, IChatRoomRepository chatRoomRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
    }

    // Chat Messages
    public async Task<List<ChatMessageDto>> GetRecentMessagesAsync(int count = 50)
    {
        return await GetRecentMessagesAsync(null, count); // Global chat
    }

    public async Task<List<ChatMessageDto>> GetRecentMessagesAsync(Guid? chatRoomId, int count = 50)
    {
        var messages = await _messageRepository.GetRecentMessagesAsync(chatRoomId, count);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            Content = m.Content,
            Username = m.User.UserName ?? "Unknown",
            UserId = m.UserId,
            IsFromBot = m.IsFromBot,
            MessageType = (StockChatRoomWeb.Shared.DTOs.Chat.MessageType)m.MessageType,
            CreatedAt = m.CreatedAt,
            ChatRoomId = m.ChatRoomId
        }).ToList();
    }

    public async Task<ChatMessageDto> SendMessageAsync(string userId, string content)
    {
        return await SendMessageAsync(userId, content, null); // Send to global chat
    }

    public async Task<ChatMessageDto> SendMessageAsync(string userId, string content, Guid? chatRoomId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userGuid);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        // Validate chat room exists if specified
        if (chatRoomId.HasValue)
        {
            var chatRoomExists = await _chatRoomRepository.ExistsAsync(chatRoomId.Value);
            if (!chatRoomExists)
                throw new ArgumentException("Chat room not found", nameof(chatRoomId));
        }

        // Only save regular messages to database (NOT stock commands)
        var message = new ChatMessage
        {
            Content = content.Trim(),
            UserId = userGuid,
            ChatRoomId = chatRoomId,
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
            CreatedAt = savedMessage.CreatedAt,
            ChatRoomId = savedMessage.ChatRoomId
        };
    }

    // Stock Commands
    public async Task<ChatMessageDto> CreateStockCommandDisplayAsync(string userId, string content, Guid? chatRoomId = null)
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
            CreatedAt = DateTime.UtcNow,
            ChatRoomId = chatRoomId
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

    // Chat Rooms

    public async Task<List<ChatRoomDto>> GetAllChatRoomsAsync()
    {
        var chatRooms = await _chatRoomRepository.GetAllAsync();

        return chatRooms.Select(cr => new ChatRoomDto
        {
            Id = cr.Id,
            Name = cr.Name,
            CreatedAt = cr.CreatedAt
        }).ToList();
    }

    public async Task<ChatRoomDto> CreateChatRoomAsync(string userId, string name)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var user = await _userRepository.GetByIdAsync(userGuid);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var chatRoom = new ChatRoom
        {
            Name = name.Trim(),
            UserId = userGuid,
            User = user
        };

        var savedChatRoom = await _chatRoomRepository.CreateAsync(chatRoom);

        return new ChatRoomDto
        {
            Id = savedChatRoom.Id,
            Name = savedChatRoom.Name,
            CreatedAt = savedChatRoom.CreatedAt
        };
    }

    public async Task<ChatRoomDto?> GetChatRoomAsync(Guid chatRoomId)
    {
        var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (chatRoom == null) return null;

        return new ChatRoomDto
        {
            Id = chatRoom.Id,
            Name = chatRoom.Name,
            CreatedAt = chatRoom.CreatedAt
        };
    }
}