using Microsoft.AspNetCore.Http;

namespace FreelanceJobBoard.Application.Interfaces.Services;
public interface ICloudinaryService
{
	Task<string> UploadFileAsync(IFormFile file, string folderName);
}
