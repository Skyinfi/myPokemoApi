using MyPokemoApi.Models.DTOs;

namespace MyPokemoApi.Services;

public interface IAuthenticationService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<GetMeResponseDto> GetCurrentUserAsync(string userId);
}