namespace FreelanceJobBoard.Application.Features.Admin.DTOs;

public class ReviewDto
{
	public int Rating { get; set; }
	public string Comment { get; set; }
	public DateTime CreatedAt { get; set; }
}