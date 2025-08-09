using MediatR;
using System.Text.Json.Serialization;

namespace FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
public class CreateJobCommand : IRequest<int>
{
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string? Tags { get; set; }
	public IEnumerable<int> SkillIds { get; set; } = new List<int>();
	public IEnumerable<int> CategoryIds { get; set; } = new List<int>();
}
