using FreelanceJobBoard.Application.DTOs;
using FreelanceJobBoard.Application.Features.Auth.Commands.Login;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.LoginDto.Email, request.LoginDto.Password);
    }
}