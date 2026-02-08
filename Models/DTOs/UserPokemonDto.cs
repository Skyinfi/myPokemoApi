namespace MyPokemoApi.Models.DTOs;

public class UserPokemonDto
{
    public string UserId { get; set; } = string.Empty;
    public int PokemonId { get; set; }
    public DateTime CaughtAt { get; set; }
    public string? Nickname { get; set; }
    public bool IsFavorite { get; set; }
    
    // 游戏机制属性
    public int Level { get; set; }
    public int Experience { get; set; }
    public int ExperienceToNextLevel { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int BattlesWon { get; set; }
    public int BattlesLost { get; set; }
    public DateTime? LastBattleAt { get; set; }
    
    public PokemonDto Pokemon { get; set; } = null!;
}