using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.DTOs;

public class TrainingRequestDto
{
    [Required]
    public string TrainingType { get; set; } = string.Empty; // "experience", "health"
    
    [Range(1, 1000)]
    public int Amount { get; set; }
}