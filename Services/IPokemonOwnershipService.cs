using MyPokemoApi.Models.DTOs;

namespace MyPokemoApi.Services;

public interface IPokemonOwnershipService
{
    Task<UserPokemonDto> CatchPokemonAsync(string userId, int pokemonId, string? nickname = null);
    Task<PagedResult<UserPokemonDto>> GetUserPokemonAsync(string userId, PokemonQueryParameters parameters);
    Task<bool> ReleasePokemonAsync(string userId, int pokemonId);
    Task<UserPokemonDto> UpdatePokemonAsync(string userId, int pokemonId, UpdateUserPokemonDto updateDto);
    Task<bool> HasPokemonAsync(string userId, int pokemonId);
    Task<int> GetPokemonCountAsync(string userId);
    Task<UserPokemonStatsDto> GetUserPokemonStatsAsync(string userId);
    Task<bool> BulkHealPokemonAsync(string userId);
    Task<int> BulkLevelUpPokemonAsync(string userId, int levelsToAdd = 1);
    
    // 游戏机制方法
    Task<TrainingResultDto> TrainPokemonAsync(string userId, int pokemonId, TrainingRequestDto request);
    Task<BattleResultDto> BattlePokemonAsync(string userId, int pokemonId, BattleRequestDto request);
    Task<HealResultDto> HealPokemonAsync(string userId, int pokemonId);
    Task<UserPokemonDto> LevelUpPokemonAsync(string userId, int pokemonId);
}