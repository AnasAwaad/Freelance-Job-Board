namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class FreelancerProfileDto
{
    public int Id { get; set; }
    public string Bio { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? AvailabilityStatus { get; set; }
    public decimal? AverageRating { get; set; }
    public int? TotalReviews { get; set; }
    public List<SkillDto> Skills { get; set; } = new();
    public List<CertificationDto> Certifications { get; set; } = new();
}