namespace FreelanceJobBoard.Application.Features.Admin.DTOs;
public class ClientDto
{
	public int Id { get; set; }
	public string FullName { get; set; }
	public decimal AverageRating { get; set; }
	public int TotalReviews { get; set; }
	public string? ProfileImageUrl { get; set; }
}