using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Full Name is required")]
    [Display(Name = "Full Name")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Profile Photo")]
    public IFormFile? ProfilePhoto { get; set; }

    public string? CurrentProfileImageUrl { get; set; }
    public string? Role { get; set; }

    // Freelancer specific fields
    [Display(Name = "Professional Bio")]
    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? Bio { get; set; }

    [Display(Name = "Description")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Display(Name = "Hourly Rate ($)")]
    [Range(1, 10000, ErrorMessage = "Hourly rate must be between $1 and $10,000")]
    public decimal? HourlyRate { get; set; }

    [Display(Name = "Availability Status")]
    public string? AvailabilityStatus { get; set; }

    // Client specific fields
    [Display(Name = "Company Name")]
    [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string? CompanyName { get; set; }

    [Display(Name = "Company Description")]
    [StringLength(1000, ErrorMessage = "Company description cannot exceed 1000 characters")]
    public string? CompanyDescription { get; set; }

    [Display(Name = "Company Website")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? CompanyWebsiteUrl { get; set; }

    [Display(Name = "Industry")]
    public string? CompanyIndustry { get; set; }
}