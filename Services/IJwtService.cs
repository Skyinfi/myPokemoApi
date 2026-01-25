using System.Security.Claims;
using MyPokemoApi.Models.Entities;

namespace MyPokemoApi.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}