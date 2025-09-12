using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockChatRoomWeb.Api.Controllers;
using StockChatRoomWeb.Shared.Common;
using StockChatRoomWeb.Shared.DTOs.ChatRoom;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Tests.Controllers;

public class ChatRoomControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly ChatRoomController _controller;
    private readonly string _testUserId = Guid.NewGuid().ToString();

    public ChatRoomControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _controller = new ChatRoomController(_mockChatService.Object);

        SetupAuthentication();
    }

    private void SetupAuthentication()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetAllChatRooms_ReturnsSuccessWithChatRooms()
    {
        var chatRooms = new List<ChatRoomDto>
        {
            new ChatRoomDto { Id = Guid.NewGuid(), Name = "General", CreatedAt = DateTime.UtcNow },
            new ChatRoomDto { Id = Guid.NewGuid(), Name = "Tech Talk", CreatedAt = DateTime.UtcNow }
        };

        _mockChatService.Setup(x => x.GetAllChatRoomsAsync())
            .ReturnsAsync(chatRooms);

        var result = await _controller.GetAllChatRooms();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<ChatRoomDto>>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
        Assert.Equal("Chat rooms retrieved successfully", response.Message);

        _mockChatService.Verify(x => x.GetAllChatRoomsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllChatRooms_ServiceThrowsException_ReturnsBadRequest()
    {
        _mockChatService.Setup(x => x.GetAllChatRoomsAsync())
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetAllChatRooms();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<ChatRoomDto>>>(badRequestResult.Value);
        
        Assert.False(response.Success);
        Assert.Contains("Failed to get chat rooms", response.Message);
        Assert.Contains("Database error", response.Message);
    }

    [Fact]
    public async Task CreateChatRoom_ValidRequest_ReturnsCreatedResult()
    {
        var request = new CreateChatRoomRequest { Name = "New Room" };
        var createdRoom = new ChatRoomDto 
        { 
            Id = Guid.NewGuid(), 
            Name = "New Room", 
            CreatedAt = DateTime.UtcNow 
        };

        _mockChatService.Setup(x => x.CreateChatRoomAsync(_testUserId, request.Name))
            .ReturnsAsync(createdRoom);

        var result = await _controller.CreateChatRoom(request);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(createdAtActionResult.Value);
        
        Assert.True(response.Success);
        Assert.Equal("New Room", response.Data!.Name);
        Assert.Equal("Chat room created successfully", response.Message);
        Assert.Equal(nameof(_controller.GetChatRoom), createdAtActionResult.ActionName);

        _mockChatService.Verify(x => x.CreateChatRoomAsync(_testUserId, request.Name), Times.Once);
    }

    [Fact]
    public async Task CreateChatRoom_UnauthenticatedUser_ReturnsUnauthorized()
    {
        var request = new CreateChatRoomRequest { Name = "New Room" };

        // Remove authentication
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // Empty identity
            }
        };

        var result = await _controller.CreateChatRoom(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(unauthorizedResult.Value);
        
        Assert.False(response.Success);
        Assert.Equal("User not authenticated", response.Message);

        _mockChatService.Verify(x => x.CreateChatRoomAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateChatRoom_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var request = new CreateChatRoomRequest { Name = "New Room" };

        _mockChatService.Setup(x => x.CreateChatRoomAsync(_testUserId, request.Name))
            .ThrowsAsync(new ArgumentException("User not found"));

        var result = await _controller.CreateChatRoom(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(badRequestResult.Value);
        
        Assert.False(response.Success);
        Assert.Equal("User not found", response.Message);
    }

    [Fact]
    public async Task CreateChatRoom_ServiceThrowsGenericException_ReturnsBadRequest()
    {
        var request = new CreateChatRoomRequest { Name = "New Room" };

        _mockChatService.Setup(x => x.CreateChatRoomAsync(_testUserId, request.Name))
            .ThrowsAsync(new Exception("Database connection failed"));

        var result = await _controller.CreateChatRoom(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(badRequestResult.Value);
        
        Assert.False(response.Success);
        Assert.Contains("Failed to create chat room", response.Message);
        Assert.Contains("Database connection failed", response.Message);
    }

    [Fact]
    public async Task GetChatRoom_ExistingId_ReturnsSuccessWithChatRoom()
    {
        var chatRoomId = Guid.NewGuid();
        var chatRoom = new ChatRoomDto 
        { 
            Id = chatRoomId, 
            Name = "Test Room", 
            CreatedAt = DateTime.UtcNow 
        };

        _mockChatService.Setup(x => x.GetChatRoomAsync(chatRoomId))
            .ReturnsAsync(chatRoom);

        var result = await _controller.GetChatRoom(chatRoomId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(okResult.Value);
        
        Assert.True(response.Success);
        Assert.Equal("Test Room", response.Data!.Name);
        Assert.Equal(chatRoomId, response.Data.Id);
        Assert.Equal("Chat room retrieved successfully", response.Message);

        _mockChatService.Verify(x => x.GetChatRoomAsync(chatRoomId), Times.Once);
    }

    [Fact]
    public async Task GetChatRoom_NonExistingId_ReturnsNotFound()
    {
        var chatRoomId = Guid.NewGuid();

        _mockChatService.Setup(x => x.GetChatRoomAsync(chatRoomId))
            .ReturnsAsync((ChatRoomDto?)null);

        var result = await _controller.GetChatRoom(chatRoomId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(notFoundResult.Value);
        
        Assert.False(response.Success);
        Assert.Equal("Chat room not found", response.Message);

        _mockChatService.Verify(x => x.GetChatRoomAsync(chatRoomId), Times.Once);
    }

    [Fact]
    public async Task GetChatRoom_ServiceThrowsException_ReturnsBadRequest()
    {
        var chatRoomId = Guid.NewGuid();

        _mockChatService.Setup(x => x.GetChatRoomAsync(chatRoomId))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetChatRoom(chatRoomId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatRoomDto>>(badRequestResult.Value);
        
        Assert.False(response.Success);
        Assert.Contains("Failed to get chat room", response.Message);
        Assert.Contains("Database error", response.Message);
    }
}