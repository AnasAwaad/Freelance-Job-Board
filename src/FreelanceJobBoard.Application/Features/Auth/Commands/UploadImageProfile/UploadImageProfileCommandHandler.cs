using FreelanceJobBoard.Application.Interfaces.Services;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.UploadImageProfile;
public class UploadImageProfileCommandHandler(ICloudinaryService cloudinaryService) : IRequestHandler<UploadImageProfileCommand, string>
{
	public async Task<string> Handle(UploadImageProfileCommand request, CancellationToken cancellationToken)
	{
		return await cloudinaryService.UploadFileAsync(request.ImageFile, "proposals");
	}
}
