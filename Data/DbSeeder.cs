using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Models.Entities;
using MyPokemoApi.Models.Enums;
using MyPokemoApi.Services;

namespace MyPokemoApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordService passwordService)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed Pokemon data if empty
        if (!await context.Pokemons.AnyAsync())
        {
            var pokemons = new List<Pokemon>
            {
                new Pokemon
                {
                    Id = 1,
                    Order = "1",
                    Name = "bulbasaur",
                    Height = "7",
                    Weight = "69",
                    Sprites = new PokemonSprites
                    {
                        FrontDefault = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/1.png",
                        FrontShiny = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/1.png"
                    }
                },
                new Pokemon
                {
                    Id = 4,
                    Order = "5",
                    Name = "charmander",
                    Height = "6",
                    Weight = "85",
                    Sprites = new PokemonSprites
                    {
                        FrontDefault = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/4.png",
                        FrontShiny = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/4.png"
                    }
                },
                new Pokemon
                {
                    Id = 7,
                    Order = "10",
                    Name = "squirtle",
                    Height = "5",
                    Weight = "90",
                    Sprites = new PokemonSprites
                    {
                        FrontDefault = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/7.png",
                        FrontShiny = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/7.png"
                    }
                },
                new Pokemon
                {
                    Id = 25,
                    Order = "32",
                    Name = "pikachu",
                    Height = "4",
                    Weight = "60",
                    Sprites = new PokemonSprites
                    {
                        FrontDefault = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/25.png",
                        FrontShiny = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/shiny/25.png"
                    }
                }
            };

            await context.Pokemons.AddRangeAsync(pokemons);
            await context.SaveChangesAsync();
        }

        // Seed test user if empty
        if (!await context.Users.AnyAsync())
        {
            var testUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@example.com",
                Name = "Test User",
                PasswordHash = passwordService.HashPassword("password123"), // Default test password
                Language = Language.EN_US,
                FavouritePokemonId = 25,
                CaughtPokemonIds = new List<int> { 1, 25 },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
        }
    }
}