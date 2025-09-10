using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StockChatRoomWeb.Api.Hubs;
using StockChatRoomWeb.Shared.Common;
using StockChatRoomWeb.Shared.Constants;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.Stock;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMessageBrokerService _messageBrokerService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IMessageBrokerService messageBrokerService,
        IHubContext<ChatHub> hubContext,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _messageBrokerService = messageBrokerService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet("messages")]
    public async Task<ActionResult<ApiResponse<List<ChatMessageDto>>>> GetRecentMessages()
    {
        try
        {
            var messages = await _chatService.GetRecentMessagesAsync(ChatConstants.MaxMessagesCount);
            return Ok(ApiResponse<List<ChatMessageDto>>.SuccessResult(messages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent messages");
            return StatusCode(500, ApiResponse<List<ChatMessageDto>>.ErrorResult("Error retrieving messages"));
        }
    }

    [HttpPost("messages")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(SendMessageRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<ChatMessageDto>.ErrorResult("Validation failed", errors));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<ChatMessageDto>.ErrorResult("User not authenticated"));
            }

            ChatMessageDto message;
            
            // Check if it's a stock command FIRST (before saving to database)
            if (await _chatService.IsStockCommandAsync(request.Content))
            {
                // Create display DTO for stock command WITHOUT saving to database
                message = await _chatService.CreateStockCommandDisplayAsync(userId, request.Content);
                
                // Process the stock command
                var stockSymbol = await _chatService.ExtractStockSymbolAsync(request.Content);
                if (!string.IsNullOrEmpty(stockSymbol))
                {
                    var stockRequest = new StockRequestMessage
                    {
                        StockSymbol = stockSymbol,
                        RequestId = Guid.NewGuid(),
                        UserId = Guid.Parse(userId),
                        Timestamp = DateTime.UtcNow
                    };

                    await _messageBrokerService.PublishStockRequestAsync(stockRequest);
                    _logger.LogInformation("Published stock request for symbol: {Symbol}", stockSymbol);
                }
            }
            else
            {
                // Regular message - save to database
                message = await _chatService.SendMessageAsync(userId, request.Content);
            }

            // Broadcast message to all connected clients
            await _hubContext.Clients.Group(ChatConstants.ChatRoomName).SendAsync("ReceiveMessage", message);

            return Ok(ApiResponse<ChatMessageDto>.SuccessResult(message, "Message sent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return StatusCode(500, ApiResponse<ChatMessageDto>.ErrorResult("Error sending message"));
        }
    }
}