namespace MyPokemoApi.Models.DTOs;

public class BattleResultDto
{
    public string BattleResult { get; set; } = string.Empty; // "won", "lost", "draw"
    public int ExperienceGained { get; set; }
    public int HealthLost { get; set; }
    public bool LevelUp { get; set; }
    public BattleStatsDto BattleStats { get; set; } = null!;
}