using MediatR;
using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.UploadImageProfile;
public class UploadImageProfileCommand : IRequest<string>
{
	public IFormFile ImageFile { get; set; } = null!;
}
