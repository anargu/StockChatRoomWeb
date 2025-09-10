using Microsoft.AspNetCore.Mvc;
using StockChatRoomWeb.Shared.Common;
using StockChatRoomWeb.Shared.DTOs.Auth;
using StockChatRoomWeb.Shared.Interfaces.Services;

namespace StockChatRoomWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register(RegisterRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var validationErrors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse.ErrorResult("Validation failed", validationErrors));
            }

            var (result, errors) = await _authService.RegisterAsync(request);
            
            if (!result)
            {
                return BadRequest(ApiResponse.ErrorResult("Registration failed. User may already exist.", errors));
            }

            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return Ok(ApiResponse.SuccessResult("User registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Username}", request.Username);
            return StatusCode(500, ApiResponse.ErrorResult("An error occurred during registration"));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Validation failed", errors));
            }

            var result = await _authService.LoginAsync(request);
            
            if (result == null)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult("Invalid credentials"));
            }

            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(ApiResponse<LoginResponse>.SuccessResult(result, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult("An error occurred during login"));
        }
    }
}