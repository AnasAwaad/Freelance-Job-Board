using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ForgotPassword;
public class ForgotPasswordCommand : IRequest<Unit>
{
    public string Email { get; set; } = string.Empty;
}