using System.ComponentModel.DataAnnotations;
using MyPokemoApi.Models.Enums;

namespace MyPokemoApi.Models.DTOs;

public class UpdateUserRequestDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public Language? Language { get; set; }
}