namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class PublicClientDto
{
	public string FullName { get; set; } = null!;
	public string? ProfileImageUrl { get; set; }
	public decimal AverageRating { get; set; }
	public CompanyDto Company { get; set; }
}
