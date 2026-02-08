namespace MyPokemoApi.Models.DTOs;

public class HealResultDto
{
    public int PreviousHealth { get; set; }
    public int NewHealth { get; set; }
    public bool FullyHealed { get; set; }
}