using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Data;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Services;
using Xunit;

namespace MyPokemoApi.Tests.Services;

public class PokemonOwnershipServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PokemonOwnershipService _service;

    public PokemonOwnershipServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new PokemonOwnershipService(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = "test-user-1",
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashedpassword"
        };

        var pokemon = new Pokemon
        {
            Id = 1,
            Name = "Bulbasaur",
            Height = "0.7",
            Weight = "6.9",
            Order = "1"
        };

        _context.Users.Add(user);
        _context.Pokemons.Add(pokemon);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CatchPokemonAsync_ShouldCreateUserPokemon_WhenValidInput()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        var nickname = "My Bulbasaur";

        // Act
        var result = await _service.CatchPokemonAsync(userId, pokemonId, nickname);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(pokemonId, result.PokemonId);
        Assert.Equal(nickname, result.Nickname);
        Assert.Equal(1, result.Level);
        Assert.Equal(0, result.Experience);
        Assert.Equal(100, result.Health);
        Assert.Equal(100, result.MaxHealth);

        // Verify in database
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);
        Assert.NotNull(userPokemon);
    }

    [Fact]
    public async Task CatchPokemonAsync_ShouldThrowException_WhenUserAlreadyOwnsPokemon()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;

        // First catch
        await _service.CatchPokemonAsync(userId, pokemonId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CatchPokemonAsync(userId, pokemonId));
    }

    [Fact]
    public async Task CatchPokemonAsync_ShouldThrowException_WhenPokemonNotFound()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 999; // Non-existent Pokemon

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CatchPokemonAsync(userId, pokemonId));
    }

    [Fact]
    public async Task GetUserPokemonAsync_ShouldReturnPagedResult_WhenUserHasPokemon()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        await _service.CatchPokemonAsync(userId, pokemonId, "Test Pokemon");

        var parameters = new PokemonQueryParameters
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetUserPokemonAsync(userId, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(pokemonId, result.Items.First().PokemonId);
    }

    [Fact]
    public async Task ReleasePokemonAsync_ShouldReturnTrue_WhenUserOwnsPokemon()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        await _service.CatchPokemonAsync(userId, pokemonId);

        // Act
        var result = await _service.ReleasePokemonAsync(userId, pokemonId);

        // Assert
        Assert.True(result);

        // Verify removed from database
        var userPokemon = await _context.UserPokemons
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PokemonId == pokemonId);
        Assert.Null(userPokemon);
    }

    [Fact]
    public async Task ReleasePokemonAsync_ShouldReturnFalse_WhenUserDoesNotOwnPokemon()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;

        // Act
        var result = await _service.ReleasePokemonAsync(userId, pokemonId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task TrainPokemonAsync_ShouldIncreaseExperience_WhenTrainingTypeIsExperience()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        await _service.CatchPokemonAsync(userId, pokemonId);

        var request = new TrainingRequestDto
        {
            TrainingType = "experience",
            Amount = 50
        };

        // Act
        var result = await _service.TrainPokemonAsync(userId, pokemonId, request);

        // Assert
        Assert.Equal(50, result.ExperienceGained);
        Assert.Equal(50, result.NewExperience);
        Assert.False(result.LevelUp);
    }

    [Fact]
    public async Task TrainPokemonAsync_ShouldLevelUp_WhenExperienceExceedsThreshold()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        await _service.CatchPokemonAsync(userId, pokemonId);

        var request = new TrainingRequestDto
        {
            TrainingType = "experience",
            Amount = 150 // More than the 100 needed for level 2
        };

        // Act
        var result = await _service.TrainPokemonAsync(userId, pokemonId, request);

        // Assert
        Assert.True(result.LevelUp);
        Assert.Equal(1, result.PreviousLevel);
        Assert.Equal(2, result.NewLevel);
        Assert.Equal(50, result.NewExperience); // 150 - 100 = 50 remaining
    }

    [Fact]
    public async Task HealPokemonAsync_ShouldRestoreFullHealth()
    {
        // Arrange
        var userId = "test-user-1";
        var pokemonId = 1;
        await _service.CatchPokemonAsync(userId, pokemonId);

        // Damage the Pokemon first
        var userPokemon = await _context.UserPokemons
            .FirstAsync(up => up.UserId == userId && up.PokemonId == pokemonId);
        userPokemon.Health = 50;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HealPokemonAsync(userId, pokemonId);

        // Assert
        Assert.Equal(50, result.PreviousHealth);
        Assert.Equal(100, result.NewHealth);
        Assert.True(result.FullyHealed);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}