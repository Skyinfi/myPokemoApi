using System.ComponentModel.DataAnnotations;
using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Models.DTOs;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public Language? Language { get; set; }

    [Required]
    public int FavouritePokemonId { get; set; }

    [Required]
    public bool Terms { get; set; }
}