using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.DTOs;

public class CatchPokemonRequestDto
{
    [Required]
    public int PokemonId { get; set; }
}