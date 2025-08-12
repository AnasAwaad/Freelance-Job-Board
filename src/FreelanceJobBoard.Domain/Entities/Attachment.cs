using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Attachment : BaseEntity
{
	public int Id { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public string FileType { get; set; } = string.Empty;
	public long FileSize { get; set; }

	public ICollection<ProposalAttachment> ProposalAttachments { get; set; } = new List<ProposalAttachment>();
	public ICollection<JobAttachment> JobAttachments { get; set; } = new List<JobAttachment>();
}

