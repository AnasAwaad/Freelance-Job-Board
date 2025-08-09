namespace FreelanceJobBoard.Application.Features.User.DTOs;
public class GetProfileResponse
{
    public UserProfileDto UserProfile { get; set; } = null!;
    public FreelancerProfileDto? FreelancerProfile { get; set; }
    public ClientProfileDto? ClientProfile { get; set; }
}

