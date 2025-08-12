using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Contract : BaseEntity
{
	public int Id { get; set; }
	public int ProposalId { get; set; }
	public int ClientId { get; set; }
	public int FreelancerId { get; set; }
	public DateTime StartTime { get; set; }
	public string? AgreedPaymentType { get; set; }
	public decimal PaymentAmount { get; set; }
	public DateTime? EndTime { get; set; }
    public int ContractStatusId { get; set; }
    public ContractStatus ContractStatus { get; set; } = null!;
    
    // Track who requested completion for approval workflow
    public string? CompletionRequestedByUserId { get; set; }
    public DateTime? CompletionRequestedAt { get; set; }

	public Proposal Proposal { get; set; } = null!;
	public Client Client { get; set; } = null!;
	public Freelancer Freelancer { get; set; } = null!;
	
	// Contract versioning relationships
	public ICollection<ContractVersion> Versions { get; set; } = new List<ContractVersion>();
	public ICollection<ContractChangeRequest> ChangeRequests { get; set; } = new List<ContractChangeRequest>();
}
