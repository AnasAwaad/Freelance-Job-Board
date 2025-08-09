namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class ClientProfileDto
{
    public int Id { get; set; }
    public CompanyDto Company { get; set; } = null!;
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
}
