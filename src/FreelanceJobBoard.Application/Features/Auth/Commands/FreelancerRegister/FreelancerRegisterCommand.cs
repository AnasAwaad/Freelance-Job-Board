using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister
{
    public class FreelancerRegisterCommand : IRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

    }
}
