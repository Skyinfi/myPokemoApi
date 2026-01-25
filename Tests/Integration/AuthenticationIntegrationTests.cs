using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenValidRequest()
    {
        // **Validates: Requirements 1.1, 1.4**
        // Property: Valid registration should succeed and return user data with tokens
        
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
        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<RegisterResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(request.Email, result.Data.User.Email);
        Assert.Equal(request.Name, result.Data.User.Name);
        Assert.NotNull(result.Data.AccessToken);
        Assert.NotNull(result.Data.RefreshToken);
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // **Validates: Requirements 1.2**
        // Property: Duplicate email registration should be rejected
        
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "duplicate@example.com",
            Password = "TestPassword123",
            Name = "Test User",
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };

        // Act - Register first user
        var firstResponse = await _client.PostAsJsonAsync("/api/authentication/register", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - Try to register with same email
        var secondResponse = await _client.PostAsJsonAsync("/api/authentication/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenValidCredentials()
    {
        // **Validates: Requirements 2.1, 2.3**
        // Property: Valid login credentials should return tokens
        
        // Arrange - First register a user
        var registerRequest = new RegisterRequestDto
        {
            Email = "login@example.com",
            Password = "LoginPassword123",
            Name = "Login User",
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };
        await _client.PostAsJsonAsync("/api/authentication/register", registerRequest);

        var loginRequest = new LoginRequestDto
        {
            Email = "login@example.com",
            Password = "LoginPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(loginRequest.Email, result.Data.User.Email);
        Assert.NotNull(result.Data.AccessToken);
        Assert.NotNull(result.Data.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // **Validates: Requirements 2.4**
        // Property: Invalid credentials should be rejected
        
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnOk_WhenValidToken()
    {
        // **Validates: Requirements 5.4**
        // Property: Valid token should allow access to protected endpoints
        
        // Arrange - Register and get token
        var registerRequest = new RegisterRequestDto
        {
            Email = "getme@example.com",
            Password = "GetMePassword123",
            Name = "GetMe User",
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };
        
        var registerResponse = await _client.PostAsJsonAsync("/api/authentication/register", registerRequest);
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var registerResult = JsonSerializer.Deserialize<ApiResponse<RegisterResponseDto>>(registerContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Add authorization header
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerResult!.Data!.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/authentication/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<GetMeResponseDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(registerRequest.Email, result.Data.User.Email);
        Assert.Equal(registerRequest.Name, result.Data.User.Name);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenNoToken()
    {
        // **Validates: Requirements 4.2**
        // Property: Protected endpoints should reject requests without valid tokens
        
        // Act
        var response = await _client.GetAsync("/api/authentication/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenInvalidToken()
    {
        // **Validates: Requirements 4.2**
        // Property: Protected endpoints should reject invalid tokens
        
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.token.here");

        // Act
        var response = await _client.GetAsync("/api/authentication/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Password123", "Name")]
    [InlineData("invalid-email", "Password123", "Name")]
    [InlineData("test@example.com", "short", "Name")]
    [InlineData("test@example.com", "Password123", "")]
    public async Task Register_ShouldReturnBadRequest_WhenInvalidInput(string email, string password, string name)
    {
        // **Validates: Requirements 1.3**
        // Property: Invalid input should be rejected with appropriate error messages
        
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = email,
            Password = password,
            Name = name,
            Language = Language.EN_US,
            FavouritePokemonId = 1,
            Terms = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}