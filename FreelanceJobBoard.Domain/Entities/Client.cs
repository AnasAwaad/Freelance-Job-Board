using FreelanceJobBoard.Domain.Common;
using FreelanceJobBoard.Domain.Identity;

namespace FreelanceJobBoard.Domain.Entities;
public class Client : BaseEntity
{
	public int Id { get; set; }
	public Company Company { get; set; }
	public decimal AverageRating { get; set; }
	public int TotalReviews { get; set; }
	public string UserId { get; set; }
	public ApplicationUser User { get; set; }
	public ICollection<Job> Jobs { get; set; }
	public ICollection<Proposal> Proposals { get; set; }
	public ICollection<Contract> Contracts { get; set; }
}

