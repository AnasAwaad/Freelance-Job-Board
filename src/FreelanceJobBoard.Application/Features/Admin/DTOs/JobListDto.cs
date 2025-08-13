namespace FreelanceJobBoard.Application.Features.Admin.DTOs;
public class JobListDto
{
	public int Id { get; set; }
	public string Title { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; }
	public string? ClientName { get; set; }
	public DateTime CreatedOn { get; set; }
}
