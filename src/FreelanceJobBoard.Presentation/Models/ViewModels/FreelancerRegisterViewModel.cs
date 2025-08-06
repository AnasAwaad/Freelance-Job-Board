using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class FreelancerRegisterViewModel
{
	[EmailAddress]
	public string Email { get; set; } = null!;

	[Required]
	[DataType(DataType.Password)]
	public string Password { get; set; } = null!;

	[Required]
	[Display(Name = "Confirm Password")]
	[DataType(DataType.Password)]
	[Compare("Password", ErrorMessage = "Passwords do not match.")]
	public string ConfirmPassword { get; set; } = null!;

	[Required]
	[Display(Name = "Full Name")]
	public string FullName { get; set; } = null!;
}
