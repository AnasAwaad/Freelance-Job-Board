using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
//public class ProposalAttachment : BaseEntity
public class ProposalAttachment
{
	public int ProposalId { get; set; }
	public int AttachmentId { get; set; }

	public Proposal Proposal { get; set; }
	public Attachment Attachment { get; set; }
}
