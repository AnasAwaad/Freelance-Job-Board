namespace FreelanceJobBoard.Application.Features.Jobs.DTOs;
public class JobSearchDto
{
	public int Id { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public string BudgetMin { get; set; }
	public string BudgetMax { get; set; }
	public string Deadline { get; set; }
	public string ClientName { get; set; }
	public DateTime CreatedOn { get; set; }
}
