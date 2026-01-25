using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPokemoApi.Data;

namespace MyPokemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("pokemon")]
    public async Task<IActionResult> GetPokemon()
    {
        var pokemon = await _context.Pokemons.ToListAsync();
        return Ok(pokemon);
    }

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var userCount = await _context.Users.CountAsync();
            var pokemonCount = await _context.Pokemons.CountAsync();
            
            return Ok(new
            {
                Status = "Healthy",
                Database = "Connected",
                UserCount = userCount,
                PokemonCount = pokemonCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Database = "Disconnected",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}