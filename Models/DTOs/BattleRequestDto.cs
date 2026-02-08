using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.DTOs;

public class BattleRequestDto
{
    [Required]
    public int OpponentPokemonId { get; set; }
    
    [Required]
    public string BattleType { get; set; } = string.Empty; // "training", "pvp", "gym"
}