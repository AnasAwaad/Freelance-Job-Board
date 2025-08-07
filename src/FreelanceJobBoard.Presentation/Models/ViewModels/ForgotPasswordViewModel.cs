using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}