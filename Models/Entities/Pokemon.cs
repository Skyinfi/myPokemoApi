using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.Entities;

public class Pokemon
{
    [Key]
    public int Id { get; set; }

    public string Order { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Height { get; set; } = string.Empty;

    public string Weight { get; set; } = string.Empty;

    public PokemonSprites Sprites { get; set; } = new();
    
    // 导航属性
    public ICollection<UserPokemon> UserPokemons { get; set; } = new List<UserPokemon>();
}

public class PokemonSprites
{
    public string FrontDefault { get; set; } = string.Empty;
    public string FrontShiny { get; set; } = string.Empty;
}