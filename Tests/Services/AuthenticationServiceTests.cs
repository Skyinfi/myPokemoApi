using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Models.Enums;
using MyPokemoApi.Services;

namespace MyPokemoApi.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationService _authenticationService;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthenticationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long-for-security",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        });
        var configuration = configurationBuilder.Build();

        _passwordService = new PasswordService();
        _jwtService = new JwtService(configuration);
        _authenticationService = new AuthenticationService(_context, _passwordService, _jwtService);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValidRequest()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "TestPassword123",
            Name = "Test User",
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };

        // Act
        var result = await _authenticationService.RegisterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal(request.Email, result.User.Email);
        Assert.Equal(request.Name, result.User.Name);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);

        // Verify user was saved to database
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(savedUser);
        Assert.True(_passwordService.VerifyPassword(request.Password, savedUser.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "existing@example.com",
            Name = "Existing User",
            PasswordHash = _passwordService.HashPassword("ExistingPassword123"),
            Language = Language.EN_US
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "NewPassword123",
            Name = "New User",
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authenticationService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "TestPassword123";
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = _passwordService.HashPassword(password),
            Language = Language.EN_US
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        // Act
        var result = await _authenticationService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.Name, result.User.Name);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenEmailNotFound()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "TestPassword123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authenticationService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenPasswordIsWrong()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = _passwordService.HashPassword("CorrectPassword123"),
            Language = Language.EN_US
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequestDto
        {
            Email = user.Email,
            Password = "WrongPassword123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authenticationService.LoginAsync(request));
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = _passwordService.HashPassword("TestPassword123"),
            Language = Language.EN_US
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authenticationService.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.User);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal(user.Name, result.User.Name);
    }

    [Fact]
    public async Task GetCurrentUserAsync_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid().ToString();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authenticationService.GetCurrentUserAsync(nonExistentUserId));
    }

    [Theory]
    [InlineData("user1@example.com", "User One")]
    [InlineData("user2@example.com", "User Two")]
    [InlineData("user3@example.com", "User Three")]
    public async Task UserRegistration_ShouldEnforceUniqueness(string email, string name)
    {
        // **Validates: Requirements 1.2**
        // Property: Email addresses must be unique across all users
        
        // Arrange
        var request1 = new RegisterRequestDto
        {
            Email = email,
            Password = "Password123",
            Name = name,
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };

        var request2 = new RegisterRequestDto
        {
            Email = email, // Same email
            Password = "DifferentPassword456",
            Name = "Different Name",
            Language = Language.ES_ES,
            FavouritePokemonId = 2,
            Terms = true
        };

        // Act
        var result1 = await _authenticationService.RegisterAsync(request1);
        
        // Assert
        Assert.NotNull(result1);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authenticationService.RegisterAsync(request2));
    }

    [Theory]
    [InlineData("correct@example.com", "CorrectPassword123", "wrong@example.com", "CorrectPassword123")]
    [InlineData("correct@example.com", "CorrectPassword123", "correct@example.com", "WrongPassword123")]
    public async Task LoginCredentials_ShouldBeValidated(string correctEmail, string correctPassword, string attemptEmail, string attemptPassword)
    {
        // **Validates: Requirements 2.2**
        // Property: Login should succeed only with correct email and password combination
        
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = correctEmail,
            Name = "Test User",
            PasswordHash = _passwordService.HashPassword(correctPassword),
            Language = Language.EN_US
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var correctRequest = new LoginRequestDto
        {
            Email = correctEmail,
            Password = correctPassword
        };

        var incorrectRequest = new LoginRequestDto
        {
            Email = attemptEmail,
            Password = attemptPassword
        };

        // Act & Assert
        var result = await _authenticationService.LoginAsync(correctRequest);
        Assert.NotNull(result);
        
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authenticationService.LoginAsync(incorrectRequest));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}