using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Xunit;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Models.Enums;
using MyPokemoApi.Services;

namespace MyPokemoApi.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public JwtServiceTests()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long-for-security",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpirationDays"] = "7"
        });
        _configuration = configurationBuilder.Build();
        _jwtService = new JwtService(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Language = Language.EN_US
        };

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT tokens contain dots
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token1);
        Assert.NotNull(token2);
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void ValidateToken_ShouldReturnPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Language = Language.EN_US
        };
        var token = _jwtService.GenerateAccessToken(user);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.Equal(user.Id, principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal(user.Name, principal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void GetUserIdFromToken_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Language = Language.EN_US
        };
        var token = _jwtService.GenerateAccessToken(user);

        // Act
        var userId = _jwtService.GetUserIdFromToken(token);

        // Assert
        Assert.Equal(user.Id, userId);
    }

    [Fact]
    public void GetUserIdFromToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _jwtService.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Theory]
    [InlineData("user1@example.com", "User One")]
    [InlineData("user2@example.com", "User Two")]
    [InlineData("user3@example.com", "User Three")]
    public void TokenGeneration_ShouldBeConsistent(string email, string name)
    {
        // **Validates: Requirements 4.1**
        // Property: Token generation and validation should be consistent
        
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            Name = name,
            Language = Language.EN_US
        };

        // Act
        var token = _jwtService.GenerateAccessToken(user);
        var principal = _jwtService.ValidateToken(token);
        var extractedUserId = _jwtService.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(token);
        Assert.NotNull(principal);
        Assert.Equal(user.Id, extractedUserId);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal(user.Name, principal.FindFirst(ClaimTypes.Name)?.Value);
    }

    [Fact]
    public void TokenValidation_ShouldRejectTamperedTokens()
    {
        // **Validates: Requirements 4.4**
        // Property: Invalid or tampered tokens should be rejected
        
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            Name = "Test User",
            Language = Language.EN_US
        };
        var validToken = _jwtService.GenerateAccessToken(user);
        
        // Tamper with the token
        var tamperedToken = validToken.Substring(0, validToken.Length - 5) + "XXXXX";

        // Act
        var validPrincipal = _jwtService.ValidateToken(validToken);
        var tamperedPrincipal = _jwtService.ValidateToken(tamperedToken);

        // Assert
        Assert.NotNull(validPrincipal);
        Assert.Null(tamperedPrincipal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not.a.jwt")]
    [InlineData("invalid")]
    [InlineData("too.short")]
    public void TokenValidation_ShouldRejectInvalidFormats(string invalidToken)
    {
        // **Validates: Requirements 4.4**
        // Property: Malformed tokens should be rejected
        
        // Act
        var principal = _jwtService.ValidateToken(invalidToken);
        var userId = _jwtService.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(principal);
        Assert.Null(userId);
    }
}