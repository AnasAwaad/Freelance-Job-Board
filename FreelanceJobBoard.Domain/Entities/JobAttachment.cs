using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class JobAttachment : BaseEntity
{
	public int JobId { get; set; }
	public int AttachmentId { get; set; }

	public Job Job { get; set; }
	public Attachment Attachment { get; set; }
}