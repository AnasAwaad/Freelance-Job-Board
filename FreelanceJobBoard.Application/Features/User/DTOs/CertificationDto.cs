namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class CertificationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Provider { get; set; }
    public string? Description { get; set; }
    public DateTime DateEarned { get; set; }
    public string? CertificationLink { get; set; }
}