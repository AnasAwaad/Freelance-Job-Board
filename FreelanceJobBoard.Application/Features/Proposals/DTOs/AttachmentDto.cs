namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class AttachmentDto
{
	public int Id { get; set; }
	public string FileName { get; set; } = null!;
	public string Url { get; set; } = null!;
}
