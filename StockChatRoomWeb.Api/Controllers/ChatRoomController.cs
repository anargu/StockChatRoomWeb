using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockChatRoomWeb.Shared.Common;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.ChatRoom;
using StockChatRoomWeb.Shared.Interfaces.Services;
using System.Security.Claims;

namespace StockChatRoomWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatRoomController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatRoomController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ChatRoomDto>>>> GetAllChatRooms()
    {
        try
        {
            var chatRooms = await _chatService.GetAllChatRoomsAsync();
            return Ok(ApiResponse<List<ChatRoomDto>>.SuccessResult(chatRooms, "Chat rooms retrieved successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ChatRoomDto>>.ErrorResult($"Failed to get chat rooms: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> GetChatRoom(Guid id)
    {
        try
        {
            var chatRoom = await _chatService.GetChatRoomAsync(id);
            if (chatRoom == null)
                return NotFound(ApiResponse<ChatRoomDto>.ErrorResult("Chat room not found"));

            return Ok(ApiResponse<ChatRoomDto>.SuccessResult(chatRoom, "Chat room retrieved successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ChatRoomDto>.ErrorResult($"Failed to get chat room: {ex.Message}"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChatRoomDto>>> CreateChatRoom([FromBody] CreateChatRoomRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<ChatRoomDto>.ErrorResult("User not authenticated"));

            var chatRoom = await _chatService.CreateChatRoomAsync(userId, request.Name);
            return CreatedAtAction(
                nameof(GetChatRoom), 
                new { id = chatRoom.Id }, 
                ApiResponse<ChatRoomDto>.SuccessResult(chatRoom, "Chat room created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ChatRoomDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ChatRoomDto>.ErrorResult($"Failed to create chat room: {ex.Message}"));
        }
    }
}