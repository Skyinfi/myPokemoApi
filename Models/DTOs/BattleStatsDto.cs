namespace MyPokemoApi.Models.DTOs;

public class BattleStatsDto
{
    public int BattlesWon { get; set; }
    public int BattlesLost { get; set; }
    public DateTime LastBattleAt { get; set; }
}