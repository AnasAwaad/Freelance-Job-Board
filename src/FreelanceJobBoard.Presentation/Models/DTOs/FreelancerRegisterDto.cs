namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class FreelancerRegisterDto
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? ProfilePhotoPath { get; set; }
	public string PhoneNumber { get; set; } = string.Empty;
	public string Bio { get; set; } = string.Empty;
	public int YearsOfExperience { get; set; }
	public decimal HourlyRate { get; set; }
	public string? PortfolioUrl { get; set; }
	public string? LinkedInUrl { get; set; }
	public string? GitHubUrl { get; set; }
	public string Specialization { get; set; } = string.Empty;
	public bool IsAvailable { get; set; } = true;
}
