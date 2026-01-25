using System.ComponentModel.DataAnnotations;

namespace MyPokemoApi.Models.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}