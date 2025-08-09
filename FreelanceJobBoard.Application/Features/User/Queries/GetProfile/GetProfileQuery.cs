using FreelanceJobBoard.Application.Features.User.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetProfile;
public class GetProfileQuery : IRequest<GetProfileResponse>
{
    public string UserId { get; set; } = null!;
}