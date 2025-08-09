using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Exceptions;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GetProfileQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        var userRoles = await _userManager.GetRolesAsync(user);
        var response = new GetProfileResponse
        {
            UserProfile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = userRoles.ToList()
            }
        };

        // Load Freelancer profile if user is a freelancer
        if (userRoles.Contains("Freelancer"))
        {
            var freelancer = await _unitOfWork.Freelancers.GetByUserIdWithDetailsAsync(request.UserId);
            if (freelancer != null)
            {
                response.FreelancerProfile = new FreelancerProfileDto
                {
                    Id = freelancer.Id,
                    Bio = freelancer.Bio,
                    Description = freelancer.Description,
                    HourlyRate = freelancer.HourlyRate,
                    AvailabilityStatus = freelancer.AvailabilityStatus,
                    AverageRating = freelancer.AverageRating,
                    TotalReviews = freelancer.TotalReviews,
                    Skills = freelancer.FreelancerSkills?.Select(fs => new SkillDto
                    {
                        Id = fs.Skill.Id,
                        Name = fs.Skill.Name
                    }).ToList() ?? new List<SkillDto>(),
                    Certifications = freelancer.Certifications?.Select(c => new CertificationDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Provider = c.Provider,
                        Description = c.Description,
                        DateEarned = c.DateEarned,
                        CertificationLink = c.CertificationLink
                    }).ToList() ?? new List<CertificationDto>()
                };
            }
        }

        // Load Client profile if user is a client
        if (userRoles.Contains("Client"))
        {
            var client = await _unitOfWork.Clients.GetByUserIdWithDetailsAsync(request.UserId);
            if (client != null)
            {
                response.ClientProfile = new ClientProfileDto
                {
                    Id = client.Id,
                    AverageRating = client.AverageRating,
                    TotalReviews = client.TotalReviews,
                    Company = new CompanyDto
                    {
                        Name = client.Company.Name,
                        Description = client.Company.Description,
                        LogoUrl = client.Company.LogoUrl,
                        WebsiteUrl = client.Company.WebsiteUrl,
                        Industry = client.Company.Industry
                    }
                };
            }
        }

        return response;
    }
}
