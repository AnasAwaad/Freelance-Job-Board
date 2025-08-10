namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class AttachmentDto
{
	public int Id { get; set; }
	public string FileName { get; set; } = null!;
	public string FilePath { get; set; } = null!;
	public string FileUrl { get; set; } = null!;
	public long FileSize { get; set; }
	public string ContentType { get; set; } = "";
}
