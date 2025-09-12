using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StockChatRoomWeb.Api.Controllers;
using StockChatRoomWeb.Api.Hubs;
using StockChatRoomWeb.Shared.Common;
using StockChatRoomWeb.Shared.DTOs.Chat;
using StockChatRoomWeb.Shared.DTOs.Stock;
using StockChatRoomWeb.Shared.Interfaces.Services;
using Xunit;

namespace StockChatRoomWeb.Tests.Controllers;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IMessageBrokerService> _mockMessageBrokerService;
    private readonly Mock<IHubContext<ChatHub>> _mockHubContext;
    private readonly Mock<ILogger<ChatController>> _mockLogger;
    private readonly ChatController _controller;
    private readonly string _testUserId = Guid.NewGuid().ToString();

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockMessageBrokerService = new Mock<IMessageBrokerService>();
        _mockHubContext = new Mock<IHubContext<ChatHub>>();
        _mockLogger = new Mock<ILogger<ChatController>>();

        _controller = new ChatController(
            _mockChatService.Object,
            _mockMessageBrokerService.Object,
            _mockHubContext.Object,
            _mockLogger.Object);

        SetupAuthentication();
        SetupSignalRMock();
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

    private void SetupSignalRMock()
    {
        var mockClients = new Mock<IHubClients>();
        var mockGroup = new Mock<IClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);
        mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockGroup.Object);
        // INFO: Cannot mock SendAsync directly as it's an extension method
        // The test will focus on the database save logic, not SignalR broadcasting
    }

    [Fact]
    public async Task SendMessage_WithStockCommand_DoesNotSaveToDatabase()
    {
        var stockCommand = "/stock=AAPL.US";
        var request = new SendMessageRequest { Content = stockCommand };

        var stockCommandDto = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            Content = stockCommand,
            Username = "TestUser",
            UserId = Guid.Parse(_testUserId),
            MessageType = MessageType.StockCommand,
            CreatedAt = DateTime.UtcNow
        };

        // Mock setup for stock command flow
        _mockChatService.Setup(x => x.IsStockCommandAsync(stockCommand))
            .ReturnsAsync(true);

        _mockChatService.Setup(x => x.CreateStockCommandDisplayAsync(_testUserId, stockCommand))
            .ReturnsAsync(stockCommandDto);

        _mockChatService.Setup(x => x.ExtractStockSymbolAsync(stockCommand))
            .ReturnsAsync("AAPL.US");

        _mockMessageBrokerService.Setup(x => x.PublishStockRequestAsync(It.IsAny<StockRequestMessage>()))
            .Returns(Task.CompletedTask);

        // Function to test
        var result = await _controller.SendMessage(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatMessageDto>>(okResult.Value);
        
        if (!response.Success)
        {
            throw new Exception($"Test failed - Response error: {response.Message}, Status: {okResult.StatusCode}");
        }
        
        Assert.True(response.Success);
        Assert.Equal(stockCommand, response.Data!.Content);
        Assert.Equal(MessageType.StockCommand, response.Data.MessageType);

        // Verify stock command flow - should NOT save to database
        _mockChatService.Verify(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        // Verify stock command flow - should create display DTO and publish request
        _mockChatService.Verify(x => x.CreateStockCommandDisplayAsync(_testUserId, stockCommand), Times.Once);
        _mockChatService.Verify(x => x.ExtractStockSymbolAsync(stockCommand), Times.Once);
        _mockMessageBrokerService.Verify(x => x.PublishStockRequestAsync(It.IsAny<StockRequestMessage>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WithRegularMessage_SavesToDatabase()
    {
        // Arrange
        var regularMessage = "Hello everyone!";
        var request = new SendMessageRequest { Content = regularMessage };

        var regularMessageDto = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            Content = regularMessage,
            Username = "TestUser",
            UserId = Guid.Parse(_testUserId),
            MessageType = MessageType.Normal,
            CreatedAt = DateTime.UtcNow
        };

        // Mock setup for regular message flow
        _mockChatService.Setup(x => x.IsStockCommandAsync(regularMessage))
            .ReturnsAsync(false);

        _mockChatService.Setup(x => x.SendMessageAsync(_testUserId, regularMessage, null))
            .ReturnsAsync(regularMessageDto);

        // Function to test
        var result = await _controller.SendMessage(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ChatMessageDto>>(okResult.Value);
        
        // TODO: Check what's in the response debugging
        if (!response.Success)
        {
            throw new Exception($"Test failed - Response error: {response.Message}, Status: {okResult.StatusCode}");
        }
        
        Assert.True(response.Success);
        Assert.Equal(regularMessage, response.Data!.Content);
        Assert.Equal(MessageType.Normal, response.Data.MessageType);

        // Verify regular message flow - should save to database
        _mockChatService.Verify(x => x.SendMessageAsync(_testUserId, regularMessage, null), Times.Once);
        
        // Verify regular message flow - should NOT use stock command methods
        _mockChatService.Verify(x => x.CreateStockCommandDisplayAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockChatService.Verify(x => x.ExtractStockSymbolAsync(It.IsAny<string>()), Times.Never);
        _mockMessageBrokerService.Verify(x => x.PublishStockRequestAsync(It.IsAny<StockRequestMessage>()), Times.Never);
    }
}