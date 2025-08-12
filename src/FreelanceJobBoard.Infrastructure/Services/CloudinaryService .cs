using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FreelanceJobBoard.Infrastructure.Services;
internal class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        await using var stream = file.OpenReadStream();

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folderName,
            PublicId = Guid.NewGuid().ToString()
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Cloudinary upload failed");

        return result.SecureUrl.ToString();
    }
}