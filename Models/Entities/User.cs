using System.ComponentModel.DataAnnotations;
using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Models.Entities;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public Language Language { get; set; } = Language.EN_US;

    public int FavouritePokemonId { get; set; }

    // 导航属性
    public ICollection<UserPokemon> UserPokemons { get; set; } = new List<UserPokemon>();
}
