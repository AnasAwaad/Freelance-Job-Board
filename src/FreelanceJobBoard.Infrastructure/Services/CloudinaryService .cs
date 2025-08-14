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
	private readonly HttpClient _httpClient;

	public CloudinaryService(IOptions<CloudinarySettings> config, HttpClient httpClient)
	{
		if (config == null) throw new ArgumentNullException(nameof(config));
		if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

		var account = new Account(
			config.Value.CloudName ?? throw new ArgumentException("CloudName missing"),
			config.Value.ApiKey ?? throw new ArgumentException("ApiKey missing"),
			config.Value.ApiSecret ?? throw new ArgumentException("ApiSecret missing")
		);

		_cloudinary = new Cloudinary(account);
		_httpClient = httpClient;
	}

	/// <summary>
	/// Uploads a file to Cloudinary. Supports images, videos and raw files.
	/// By default uploads are public (Type = "upload"). If makePrivate is true, we set Type = "authenticated".
	/// Returns the Secure URL (public or private). For private, the returned URL may still require server-side download.
	/// </summary>
	public async Task<string> UploadFileAsync(IFormFile file, string folderName)
	{
		bool makePrivate = false;
		if (file == null) throw new ArgumentNullException(nameof(file));
		if (string.IsNullOrWhiteSpace(folderName)) folderName = "";

		await using var stream = file.OpenReadStream();
		string publicId = Guid.NewGuid().ToString();

		// Choose upload type (public = "upload", private = "authenticated")
		var deliveryType = makePrivate ? "authenticated" : "upload";

		// Image
		if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
		{
			var imageUploadParams = new ImageUploadParams
			{
				File = new FileDescription(file.FileName, stream),
				Folder = folderName,
				PublicId = publicId,
				Type = deliveryType,     // "upload" or "authenticated"
				UseFilename = false,
				UniqueFilename = true,
				Overwrite = false
				// Do NOT set ResourceType here (it's read-only on ImageUploadParams)
			};

			var imageResult = await _cloudinary.UploadAsync(imageUploadParams);
			if (imageResult.StatusCode != System.Net.HttpStatusCode.OK)
				throw new Exception($"Cloudinary image upload failed: {imageResult.Error?.Message}");

			// For public uploads, SecureUrl is directly usable.
			// For authenticated/private uploads, SecureUrl might require server-side handling on download.
			return imageResult.SecureUrl?.ToString() ?? throw new Exception("No secure url returned");
		}

		// Video
		if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
		{
			var videoUploadParams = new VideoUploadParams
			{
				File = new FileDescription(file.FileName, stream),
				Folder = folderName,
				PublicId = publicId,
				Type = deliveryType,
				UseFilename = false,
				UniqueFilename = true,
				Overwrite = false
			};

			var videoResult = await _cloudinary.UploadAsync(videoUploadParams);
			if (videoResult.StatusCode != System.Net.HttpStatusCode.OK)
				throw new Exception($"Cloudinary video upload failed: {videoResult.Error?.Message}");

			return videoResult.SecureUrl?.ToString() ?? throw new Exception("No secure url returned");
		}

		// Raw / other files (pdf, docx, etc.)
		var rawUploadParams = new RawUploadParams
		{
			File = new FileDescription(file.FileName, stream),
			Folder = folderName,
			PublicId = publicId,
			Type = deliveryType,
			UseFilename = false,
			UniqueFilename = true,
			Overwrite = false
		};

		var rawResult = await _cloudinary.UploadAsync(rawUploadParams);
		if (rawResult.StatusCode != System.Net.HttpStatusCode.OK)
			throw new Exception($"Cloudinary raw upload failed: {rawResult.Error?.Message}");

		return rawResult.SecureUrl?.ToString() ?? throw new Exception("No secure url returned");
	}

	/// <summary>
	/// Downloads a file from a Cloudinary URL. If the URL is a standard public /upload/ URL we stream it directly.
	/// Otherwise we treat it as a possibly-private resource, resolve its public_id and ask Cloudinary (server-side)
	/// for the resource and stream the secure resource URL back to the caller.
	/// </summary>
	public async Task<Stream> DownloadFileAsync(string fileUrl)
	{
		if (string.IsNullOrEmpty(fileUrl))
			throw new ArgumentException("File URL cannot be null or empty", nameof(fileUrl));

		if (!fileUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !fileUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException("Invalid file URL format", nameof(fileUrl));

		// If it's a standard public upload URL, just stream it directly
		// Typical public URL contains "/upload/" (images, videos, raw)
		if (fileUrl.Contains("/upload/") && !fileUrl.Contains("/private/") && !fileUrl.Contains("/authenticated/"))
		{
			var response = await _httpClient.GetAsync(fileUrl);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsStreamAsync();
		}

		// Otherwise, handle as private/authenticated or non-standard Cloudinary URL:
		// 1. Extract public_id
		// 2. Call Cloudinary GetResourceAsync with correct resource type
		// 3. Download resource.SecureUrl (server-side)
		var publicId = ExtractPublicIdFromUrl(fileUrl);
		if (string.IsNullOrEmpty(publicId))
			throw new InvalidOperationException("Could not extract public ID from Cloudinary URL");

		var resourceType = DetermineResourceType(fileUrl); // enum ResourceType
		var getParams = new GetResourceParams(publicId)
		{
			ResourceType = resourceType
		};

		var resource = await _cloudinary.GetResourceAsync(getParams);
		if (resource == null || string.IsNullOrEmpty(resource.SecureUrl))
			throw new InvalidOperationException("Failed to get resource from Cloudinary");

		// Attempt to download the secure URL returned by Cloudinary (server-side)
		var authResponse = await _httpClient.GetAsync(resource.SecureUrl);
		authResponse.EnsureSuccessStatusCode();
		return await authResponse.Content.ReadAsStreamAsync();
	}

	#region Helpers

	private static string? ExtractPublicIdFromUrl(string cloudinaryUrl)
	{
		try
		{
			// Example:
			// https://res.cloudinary.com/<cloud>/raw/upload/v1234567890/folder/sub/file.pdf
			var uri = new Uri(cloudinaryUrl);
			var segments = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			// Find the index of "upload"
			var uploadIndex = Array.IndexOf(segments, "upload");
			if (uploadIndex == -1 || uploadIndex + 1 >= segments.Length)
				return null;

			// The next segment might be the version (v12345). Skip it if so.
			var startIndex = uploadIndex + 1;
			if (segments[startIndex].StartsWith("v", StringComparison.OrdinalIgnoreCase) && segments[startIndex].Length > 1 && char.IsDigit(segments[startIndex][1]))
				startIndex++;

			// Join remaining segments to form public_id (including nested folders)
			var parts = segments.Skip(startIndex).ToArray();
			if (parts.Length == 0) return null;

			// Remove file extension from last part
			var last = parts.Last();
			if (last.Contains('.'))
				parts[parts.Length - 1] = Path.GetFileNameWithoutExtension(last);

			var publicId = string.Join("/", parts);
			return publicId;
		}
		catch
		{
			return null;
		}
	}

	private static ResourceType DetermineResourceType(string cloudinaryUrl)
	{
		// Decide resource type based on URL path segment (/image/, /video/, /raw/)
		if (cloudinaryUrl.Contains("/image/")) return ResourceType.Image;
		if (cloudinaryUrl.Contains("/video/")) return ResourceType.Video;
		return ResourceType.Raw;
	}

	#endregion
}

