namespace FreelanceJobBoard.Application.Features.Jobs.DTOs;
public class RecentJobDto
{
	public int Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string ClientName { get; set; }
	public decimal BudgetMin { get; set; }
	public string? ClientProfileImage { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime CreatedDate { get; set; }
	public string Status { get; set; }
	public List<string>? Tags { get; set; }
	public DateTime CreatedOn { get; set; }
}
