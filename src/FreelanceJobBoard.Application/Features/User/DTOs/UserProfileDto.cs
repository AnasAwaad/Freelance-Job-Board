namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class UserProfileDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}