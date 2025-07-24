using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
public class CreateJobCommand : IRequest
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string? Tags { get; set; }
	public string Status { get; set; }
	public IEnumerable<int> SkillIds { get; set; }
	public IEnumerable<int> CategoryIds { get; set; }

}
