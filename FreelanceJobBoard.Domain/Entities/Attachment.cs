using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Attachment : BaseEntity
{
	public int Id { get; set; }
	public string FileName { get; set; }
	public string FilePath { get; set; }
	public string FileType { get; set; }
	public long FileSize { get; set; }

	public ICollection<ProposalAttachment> ProposalAttachments { get; set; }
	public ICollection<JobAttachment> JobAttachments { get; set; }
}

