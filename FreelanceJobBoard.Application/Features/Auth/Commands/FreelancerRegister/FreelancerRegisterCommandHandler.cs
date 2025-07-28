using FreelanceJobBoard.Application.Interfaces;
using MediatR;


namespace FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;


public class FreelancerRegisterCommandHandler : IRequestHandler<FreelancerRegisterCommand>

{
    private readonly IAuthService _authService;

    public FreelancerRegisterCommandHandler(IAuthService authService) => _authService = authService;

    public async Task Handle(FreelancerRegisterCommand request, CancellationToken cancellationToken)
    {
        await _authService.RegisterFreelancerAsync(
            email: request.Email,
            password: request.Password,
            fullName: request.FullName);

    }
}
