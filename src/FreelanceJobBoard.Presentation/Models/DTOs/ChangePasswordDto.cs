namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}