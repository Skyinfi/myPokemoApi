namespace MyPokemoApi.Models.DTOs;

public class TrainingResultDto
{
    public bool LevelUp { get; set; }
    public int PreviousLevel { get; set; }
    public int NewLevel { get; set; }
    public int ExperienceGained { get; set; }
    public int NewExperience { get; set; }
    public int NewExperienceToNextLevel { get; set; }
}