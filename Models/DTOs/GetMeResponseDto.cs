namespace MyPokemoApi.Models.DTOs;

public class GetMeResponseDto
{
    public UserDto User { get; set; } = new();
}