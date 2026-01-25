using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Services;

namespace MyPokemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<RegisterResponseDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            var result = await _authenticationService.RegisterAsync(request);

            return CreatedAtAction(nameof(GetMe), null, new ApiResponse<RegisterResponseDto>
            {
                Success = true,
                Data = result,
                Message = "User registered successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ApiResponse<RegisterResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            return StatusCode(500, new ApiResponse<RegisterResponseDto>
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            var result = await _authenticationService.LoginAsync(request);

            return Ok(new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Data = result,
                Message = "Login successful"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid credentials"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login");
            return StatusCode(500, new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<RefreshTokenResponseDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken);

            return Ok(new ApiResponse<RefreshTokenResponseDto>
            {
                Success = true,
                Data = result,
                Message = "Token refreshed successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "Invalid refresh token"
            });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "Refresh token functionality not yet implemented"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<GetMeResponseDto>>> GetMe()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse<GetMeResponseDto>
                {
                    Success = false,
                    Message = "Invalid token"
                });
            }

            var result = await _authenticationService.GetCurrentUserAsync(userId);

            return Ok(new ApiResponse<GetMeResponseDto>
            {
                Success = true,
                Data = result,
                Message = "User information retrieved successfully"
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ApiResponse<GetMeResponseDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user information");
            return StatusCode(500, new ApiResponse<GetMeResponseDto>
            {
                Success = false,
                Message = "An error occurred while retrieving user information"
            });
        }
    }
}