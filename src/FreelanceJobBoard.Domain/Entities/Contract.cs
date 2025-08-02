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
    public ContractStatus ContractStatus { get; set; }

	public Proposal Proposal { get; set; }
	public Client Client { get; set; }
	public Freelancer Freelancer { get; set; }
}
