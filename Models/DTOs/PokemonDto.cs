namespace MyPokemoApi.Models.DTOs;

public class PokemonDto
{
    public int Id { get; set; }
    public string Order { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public PokemonSpritesDto Sprites { get; set; } = new();
}

public class PokemonSpritesDto
{
    public string FrontDefault { get; set; } = string.Empty;
    public string FrontShiny { get; set; } = string.Empty;
}