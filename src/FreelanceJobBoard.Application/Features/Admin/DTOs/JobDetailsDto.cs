using FreelanceJobBoard.Application.Features.Proposals.DTOs;

namespace FreelanceJobBoard.Application.Features.Admin.DTOs;
public class JobDetailsDto
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public string Status { get; set; }
	public DateTime Deadline { get; set; }
	public DateTime CreatedOn { get; set; }
	public ClientDto Client { get; set; }
	public List<ProposalDto> Proposals { get; set; }
	public ReviewDto? Review { get; set; }
}
