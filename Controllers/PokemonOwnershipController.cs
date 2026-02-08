using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPokemoApi.Models.DTOs;
using MyPokemoApi.Services;
using System.Security.Claims;

namespace MyPokemoApi.Controllers;

[ApiController]
[Route("api/users/{userId}/pokemon")]
[Authorize]
public class PokemonOwnershipController : ControllerBase
{
    private readonly IPokemonOwnershipService _pokemonOwnershipService;

    public PokemonOwnershipController(IPokemonOwnershipService pokemonOwnershipService)
    {
        _pokemonOwnershipService = pokemonOwnershipService;
    }

    [HttpPost("{pokemonId}")]
    public async Task<ActionResult<ApiResponse<UserPokemonDto>>> CatchPokemon(
        string userId, 
        int pokemonId, 
        [FromBody] CatchPokemonRequestDto? request = null)
    {
        try
        {
            // 验证用户只能操作自己的Pokemon
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.CatchPokemonAsync(userId, pokemonId, request?.Nickname);
            
            return Ok(new ApiResponse<UserPokemonDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<UserPokemonDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<UserPokemonDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserPokemonDto>>>> GetUserPokemon(
        string userId,
        [FromQuery] PokemonQueryParameters parameters)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.GetUserPokemonAsync(userId, parameters);
            
            return Ok(new ApiResponse<PagedResult<UserPokemonDto>>
            {
                Success = true,
                Data = result
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<PagedResult<UserPokemonDto>>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpDelete("{pokemonId}")]
    public async Task<ActionResult<ApiResponse<object>>> ReleasePokemon(string userId, int pokemonId)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.ReleasePokemonAsync(userId, pokemonId);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Pokemon not found or not owned by user"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Pokemon已成功释放"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPut("{pokemonId}")]
    public async Task<ActionResult<ApiResponse<UserPokemonDto>>> UpdatePokemon(
        string userId, 
        int pokemonId, 
        [FromBody] UpdateUserPokemonDto updateDto)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.UpdatePokemonAsync(userId, pokemonId, updateDto);
            
            return Ok(new ApiResponse<UserPokemonDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<UserPokemonDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<UserPokemonDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("{pokemonId}/train")]
    public async Task<ActionResult<ApiResponse<TrainingResultDto>>> TrainPokemon(
        string userId, 
        int pokemonId, 
        [FromBody] TrainingRequestDto request)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.TrainPokemonAsync(userId, pokemonId, request);
            
            return Ok(new ApiResponse<TrainingResultDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<TrainingResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<TrainingResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("{pokemonId}/battle")]
    public async Task<ActionResult<ApiResponse<BattleResultDto>>> BattlePokemon(
        string userId, 
        int pokemonId, 
        [FromBody] BattleRequestDto request)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.BattlePokemonAsync(userId, pokemonId, request);
            
            return Ok(new ApiResponse<BattleResultDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<BattleResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<BattleResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<BattleResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [HttpPost("{pokemonId}/heal")]
    public async Task<ActionResult<ApiResponse<HealResultDto>>> HealPokemon(string userId, int pokemonId)
    {
        try
        {
            if (!IsAuthorizedUser(userId))
            {
                return Forbid();
            }

            var result = await _pokemonOwnershipService.HealPokemonAsync(userId, pokemonId);
            
            return Ok(new ApiResponse<HealResultDto>
            {
                Success = true,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new ApiResponse<HealResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<HealResultDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    private bool IsAuthorizedUser(string userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return currentUserId == userId;
    }
}