namespace MyPokemoApi.Models.DTOs;

public class UserPokemonStatsDto
{
    public int TotalPokemon { get; set; }
    public int FavoritePokemon { get; set; }
    public double AverageLevel { get; set; }
    public int HighestLevel { get; set; }
    public int LowestLevel { get; set; }
    public int TotalBattlesWon { get; set; }
    public int TotalBattlesLost { get; set; }
    public int FaintedPokemon { get; set; }
    public int MaxLevelPokemon { get; set; }
    public double WinRate => TotalBattlesWon + TotalBattlesLost > 0 
        ? (double)TotalBattlesWon / (TotalBattlesWon + TotalBattlesLost) * 100 
        : 0;
}