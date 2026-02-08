using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace MyPokemoApi.Tests.Integration;

public class PokemonOwnershipIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PokemonOwnershipIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CatchPokemon_ShouldReturnSuccess_WhenValidRequest()
    {
        // Arrange
        await SeedTestData();
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var userId = "test-user-1";
        var pokemonId = 1;
        var request = new CatchPokemonRequestDto { Nickname = "My Bulbasaur" };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/users/{userId}/pokemon/{pokemonId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<UserPokemonDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(userId, result.Data.UserId);
        Assert.Equal(pokemonId, result.Data.PokemonId);
        Assert.Equal("My Bulbasaur", result.Data.Nickname);
    }

    [Fact]
    public async Task GetUserPokemon_ShouldReturnPagedResult_WhenUserHasPokemon()
    {
        // Arrange
        await SeedTestData();
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var userId = "test-user-1";
        
        // First catch a Pokemon
        var pokemonId = 1;
        var catchRequest = new CatchPokemonRequestDto { Nickname = "Test Pokemon" };
        var catchJson = JsonSerializer.Serialize(catchRequest);
        var catchContent = new StringContent(catchJson, Encoding.UTF8, "application/json");
        await _client.PostAsync($"/api/users/{userId}/pokemon/{pokemonId}", catchContent);

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}/pokemon?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<UserPokemonDto>>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalCount);
        Assert.Single(result.Data.Items);
    }

    [Fact]
    public async Task TrainPokemon_ShouldReturnTrainingResult_WhenValidRequest()
    {
        // Arrange
        await SeedTestData();
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var userId = "test-user-1";
        var pokemonId = 1;
        
        // First catch a Pokemon
        var catchRequest = new CatchPokemonRequestDto { Nickname = "Training Pokemon" };
        var catchJson = JsonSerializer.Serialize(catchRequest);
        var catchContent = new StringContent(catchJson, Encoding.UTF8, "application/json");
        await _client.PostAsync($"/api/users/{userId}/pokemon/{pokemonId}", catchContent);

        // Prepare training request
        var trainingRequest = new TrainingRequestDto
        {
            TrainingType = "experience",
            Amount = 50
        };
        var trainingJson = JsonSerializer.Serialize(trainingRequest);
        var trainingContent = new StringContent(trainingJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/api/users/{userId}/pokemon/{pokemonId}/train", trainingContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TrainingResultDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(50, result.Data.ExperienceGained);
    }

    private async Task SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        // Clear existing data
        context.UserPokemons.RemoveRange(context.UserPokemons);
        context.Users.RemoveRange(context.Users);
        context.Pokemons.RemoveRange(context.Pokemons);
        await context.SaveChangesAsync();

        // Add test user
        var user = new User
        {
            Id = "test-user-1",
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = passwordService.HashPassword("password123")
        };

        // Add test Pokemon
        var pokemon = new Pokemon
        {
            Id = 1,
            Name = "Bulbasaur",
            Height = "0.7",
            Weight = "6.9",
            Order = "1"
        };

        context.Users.Add(user);
        context.Pokemons.Add(pokemon);
        await context.SaveChangesAsync();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var json = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/authentication/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Data?.AccessToken ?? throw new InvalidOperationException("Failed to get auth token");
    }
}