using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public AuthenticationService(
        ApplicationDbContext context,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Language = request.Language ?? Language.EN_US,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new RegisterResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Language = user.Language,
                FavouritePokemonId = user.FavouritePokemonId,
                CaughtPokemonIds = user.CaughtPokemonIds.ToList(),
                CreatedAt = user.CreatedAt.ToString("o"),
                UpdatedAt = user.UpdatedAt.ToString("o")
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Language = user.Language,
                FavouritePokemonId = user.FavouritePokemonId,
                CaughtPokemonIds = user.CaughtPokemonIds.ToList(),
                CreatedAt = user.CreatedAt.ToString("o"),
                UpdatedAt = user.UpdatedAt.ToString("o")
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // In a production app, you would store refresh tokens in the database
        // and validate them here. For simplicity, we'll just generate new tokens
        // This is a simplified implementation
        throw new NotImplementedException("Refresh token functionality requires additional implementation");
    }

    public async Task<GetMeResponseDto> GetCurrentUserAsync(string userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        return new GetMeResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Language = user.Language,
                FavouritePokemonId = user.FavouritePokemonId,
                CaughtPokemonIds = user.CaughtPokemonIds.ToList(),
                CreatedAt = user.CreatedAt.ToString("o"),
                UpdatedAt = user.UpdatedAt.ToString("o")
            }
        };
    }
}