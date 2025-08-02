using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ForgotPassword;
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IEmailService _emailService;

	public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager, IEmailService emailService)
	{
		_userManager = userManager;
		_emailService = emailService;
	}

	public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByEmailAsync(request.Email);
		if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
			return Unit.Value;

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var resetLink = $"https://yourfrontend.com/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";

		var subject = "Password Reset Request";
		var body = $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>";

		await _emailService.SendEmailAsync(user.Email, subject, body);
		return Unit.Value;
	}
}
