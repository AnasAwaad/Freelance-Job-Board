namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class ConfirmEmailDto
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}