using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters long", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required")]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role")]
    [Display(Name = "Account Type")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    // Additional fields for Freelancer
    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Professional Bio")]
    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? Bio { get; set; }

    [Display(Name = "Years of Experience")]
    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    public int? YearsOfExperience { get; set; }

    [Display(Name = "Hourly Rate ($)")]
    [Range(1, 10000, ErrorMessage = "Hourly rate must be between $1 and $10,000")]
    public decimal? HourlyRate { get; set; }

    [Display(Name = "Portfolio Website")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? PortfolioUrl { get; set; }

    [Display(Name = "Primary Specialization")]
    public string? Specialization { get; set; }

    // Additional fields for Client
    [Display(Name = "Company Name")]
    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string? CompanyName { get; set; }

    [Display(Name = "Company Website")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? CompanyWebsite { get; set; }

    [Display(Name = "Industry")]
    public string? Industry { get; set; }
}