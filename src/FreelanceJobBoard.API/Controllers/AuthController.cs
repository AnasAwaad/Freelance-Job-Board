using FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
using FreelanceJobBoard.Application.Features.Auth.Commands.ConfirmEmail;
using FreelanceJobBoard.Application.Features.Auth.Commands.ForgotPassword;
using FreelanceJobBoard.Application.Features.Auth.Commands.FreelancerRegister;
using FreelanceJobBoard.Application.Features.Auth.Commands.Login;
using FreelanceJobBoard.Application.Features.Auth.Commands.ResetPassword;
using FreelanceJobBoard.Application.Features.Auth.Commands.UploadImageProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IMediator mediator, ILogger<AuthController> logger)
		{
			_mediator = mediator;
			_logger = logger;
		}

		[HttpPost("client-register")]
		public async Task<IActionResult> ClientRegister([FromBody] ClientRegisterCommand command)
		{
			await _mediator.Send(command);
			return Ok(new { success = true, message = "Freelancer registration successful" });
		}
		[HttpPost("freelancer-register")]
		public async Task<IActionResult> FreelancerRegister([FromBody] FreelancerRegisterCommand command)
		{
			await _mediator.Send(command);
			return Ok(new { success = true, message = "Freelancer registration successful" });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			var command = new LoginCommand { LoginDto = loginDto };

			return Ok(await _mediator.Send(command));
		}
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
		{
			await _mediator.Send(command);
			return Ok(new { success = true, message = "If your email exists, a reset link has been sent." });
		}
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
		{
			await _mediator.Send(command);
			return Ok(new { success = true, message = "Password reset successful" });
		}
		[HttpGet("confirm-email")]
		public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailCommand command)
		{
			await _mediator.Send(command);
			return Ok(new { success = true, message = "Email confirmed successfully" });
		}
		[HttpPost("logout")]
		public IActionResult Logout()
		{
			// I'll still put some logic in here
			return Ok(new { success = true, message = "Logged out successfully" });
		}

		[HttpPost("upload-image-profile")]
		public async Task<IActionResult> UploadImageProfile([FromForm] UploadImageProfileCommand command)
		{
			var imagePathUrl = await _mediator.Send(command);
			return Ok(imagePathUrl);
		}
	}
}