using FreelanceJobBoard.Application.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.Login
{
    public class LoginCommand : IRequest<AuthResponseDto>
    {
        public LoginDto LoginDto { get; set; } = null!;
    }
}
