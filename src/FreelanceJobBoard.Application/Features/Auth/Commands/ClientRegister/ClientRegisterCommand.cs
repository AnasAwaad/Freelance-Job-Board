using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
public class ClientRegisterCommand : IRequest
{
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? ProfilePhotoUrl { get; set; }
	public string? CompanyName { get; set; }
	public string? CompanyWebsite { get; set; }
	public string? Industry { get; set; }
}
