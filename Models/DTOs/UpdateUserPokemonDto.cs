using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.DTOs;

public class UpdateUserPokemonDto
{
    [MaxLength(50)]
    public string? Nickname { get; set; }
    
    public bool? IsFavorite { get; set; }
}