namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class PublicClientViewModel
{
	public string FullName { get; set; } = null!;
	public string? ProfileImageUrl { get; set; }
	public decimal AverageRating { get; set; }
	public CompanyViewModel Company { get; set; }
}
