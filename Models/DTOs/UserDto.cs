using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Models.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Language Language { get; set; }

    public int FavouritePokemonId { get; set; }

    public List<int> CaughtPokemonIds { get; set; } = new();

    public static UserDto FromEntity(Entities.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            CreatedAt = user.CreatedAt.ToString("o"),
            UpdatedAt = user.UpdatedAt.ToString("o"),
            Email = user.Email,
            Name = user.Name,
            Language = user.Language,
            FavouritePokemonId = user.FavouritePokemonId,
            CaughtPokemonIds = user.CaughtPokemonIds.ToList()
        };
    }
}
