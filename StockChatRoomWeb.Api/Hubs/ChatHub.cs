using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StockChatRoomWeb.Shared.Constants;

namespace StockChatRoomWeb.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatConstants.ChatRoomName);
        
        _logger.LogInformation("User {Username} ({UserId}) connected to chat. ConnectionId: {ConnectionId}", 
            username, userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatConstants.ChatRoomName);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {Username} ({UserId}) disconnected from chat with error. ConnectionId: {ConnectionId}", 
                username, userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("User {Username} ({UserId}) disconnected from chat. ConnectionId: {ConnectionId}", 
                username, userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChatRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatConstants.ChatRoomName);
        
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        _logger.LogInformation("User {Username} explicitly joined global chat room", username);
    }

    public async Task LeaveChatRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatConstants.ChatRoomName);
        
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        _logger.LogInformation("User {Username} left global chat room", username);
    }

    public async Task JoinRoom(string roomId)
    {
        var groupName = $"{ChatConstants.RoomGroupPrefix}{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        _logger.LogInformation("User {Username} joined room {RoomId}", username, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        var groupName = $"{ChatConstants.RoomGroupPrefix}{roomId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        _logger.LogInformation("User {Username} left room {RoomId}", username, roomId);
    }
}