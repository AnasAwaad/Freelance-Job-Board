using Microsoft.AspNetCore.Http;
using System.IO;

namespace FreelanceJobBoard.Application.Interfaces.Services;
public interface ICloudinaryService
{
	Task<string> UploadFileAsync(IFormFile file, string folderName );
	Task<Stream> DownloadFileAsync(string fileUrl);
}
