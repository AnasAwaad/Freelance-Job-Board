using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
public class ClientRegisterCommandHandler : IRequestHandler<ClientRegisterCommand>

{
    private readonly IAuthService _authService;

    public ClientRegisterCommandHandler(IAuthService authService) => _authService = authService;

    public async Task Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {
        await _authService.RegisterClientAsync(
            email: request.Email,
            password: request.Password,
            fullName: request.FullName);

    }
}