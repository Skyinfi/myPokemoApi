using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.Entities;

public class UserPokemon
{
    public string UserId { get; set; } = string.Empty;
    public int PokemonId { get; set; }
    public DateTime CaughtAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string? Nickname { get; set; }
    
    public bool IsFavorite { get; set; } = false;
    
    // 游戏机制属性
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExperienceToNextLevel { get; set; } = 100;
    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    
    // 战斗统计
    public int BattlesWon { get; set; } = 0;
    public int BattlesLost { get; set; } = 0;
    public DateTime? LastBattleAt { get; set; }
    
    // 导航属性
    public User User { get; set; } = null!;
    public Pokemon Pokemon { get; set; } = null!;
}