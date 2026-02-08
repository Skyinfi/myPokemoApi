namespace MyPokemoApi.Models.DTOs;

public class PokemonQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "caughtAt";
    public string SortOrder { get; set; } = "desc";
    public bool? FavoriteOnly { get; set; }
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
}