using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class FreelancerRegisterViewModel
{
	[Required(ErrorMessage = "Email is required")]
	[EmailAddress(ErrorMessage = "Please enter a valid email address")]
	[Display(Name = "Email Address")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password is required")]
	[DataType(DataType.Password)]
	[StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
	[Display(Name = "Password")]
	public string Password { get; set; } = string.Empty;

	[Required(ErrorMessage = "Please confirm your password")]
	[Display(Name = "Confirm Password")]
	[DataType(DataType.Password)]
	[Compare("Password", ErrorMessage = "Passwords do not match")]
	public string ConfirmPassword { get; set; } = string.Empty;

	[Required(ErrorMessage = "Full name is required")]
	[Display(Name = "Full Name")]
	[StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
	public string FullName { get; set; } = string.Empty;

	[Required(ErrorMessage = "Phone number is required")]
	[Display(Name = "Phone Number")]
	[Phone(ErrorMessage = "Please enter a valid phone number")]
	public string PhoneNumber { get; set; } = string.Empty;

	[Required(ErrorMessage = "Please provide a brief bio")]
	[Display(Name = "Professional Bio")]
	[StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters", MinimumLength = 50)]
	public string Bio { get; set; } = string.Empty;

	[Display(Name = "Profile Photo")]
	public IFormFile? ProfilePhoto { get; set; }

	[Required(ErrorMessage = "Years of experience is required")]
	[Display(Name = "Years of Experience")]
	[Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
	public int YearsOfExperience { get; set; }

	[Required(ErrorMessage = "Hourly rate is required")]
	[Display(Name = "Hourly Rate ($)")]
	[Range(1, 10000, ErrorMessage = "Hourly rate must be between $1 and $10,000")]
	public decimal HourlyRate { get; set; }

	[Display(Name = "Portfolio Website")]
	[Url(ErrorMessage = "Please enter a valid URL")]
	public string? PortfolioUrl { get; set; }

	[Display(Name = "LinkedIn Profile")]
	[Url(ErrorMessage = "Please enter a valid LinkedIn URL")]
	public string? LinkedInUrl { get; set; }

	[Display(Name = "GitHub Profile")]
	[Url(ErrorMessage = "Please enter a valid GitHub URL")]
	public string? GitHubUrl { get; set; }

	[Required(ErrorMessage = "Please select your primary specialization")]
	[Display(Name = "Primary Specialization")]
	public string Specialization { get; set; } = string.Empty;

	[Display(Name = "Available for Work")]
	public bool IsAvailable { get; set; } = true;
}
