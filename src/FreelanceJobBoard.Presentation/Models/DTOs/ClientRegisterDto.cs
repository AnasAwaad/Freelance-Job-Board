namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class ClientRegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyWebsite { get; set; }
    public string? Industry { get; set; }
}