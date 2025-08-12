using System.Net;
using FluentAssertions;
using FreelanceJobBoard.Infrastructure.Services;
using FreelanceJobBoard.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace FreelanceJobBoard.Application.Tests.Services;

public class CloudinaryServiceTests
{
    private readonly Mock<IOptions<CloudinarySettings>> _cloudinarySettingsMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly CloudinaryService _cloudinaryService;

    public CloudinaryServiceTests()
    {
        _cloudinarySettingsMock = new Mock<IOptions<CloudinarySettings>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

        var cloudinarySettings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret"
        };

        _cloudinarySettingsMock.Setup(x => x.Value).Returns(cloudinarySettings);

        _cloudinaryService = new CloudinaryService(_cloudinarySettingsMock.Object, _httpClient);
    }

    [Fact]
    public async Task DownloadFileAsync_WithValidUrl_ShouldReturnStream()
    {
        // Arrange
        var fileUrl = "https://cloudinary.com/test-file.pdf";
        var fileContent = "Test file content"u8.ToArray();

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(fileContent)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _cloudinaryService.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        
        using var memoryStream = new MemoryStream();
        await result.CopyToAsync(memoryStream);
        var downloadedContent = memoryStream.ToArray();
        
        downloadedContent.Should().BeEquivalentTo(fileContent);
        
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == fileUrl),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task DownloadFileAsync_WithNullUrl_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = async () => await _cloudinaryService.DownloadFileAsync(null!);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File URL cannot be null or empty*");
    }

    [Fact]
    public async Task DownloadFileAsync_WithEmptyUrl_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = async () => await _cloudinaryService.DownloadFileAsync(string.Empty);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File URL cannot be null or empty*");
    }

    [Fact]
    public async Task DownloadFileAsync_WithInvalidUrlFormat_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidUrl = "invalid-url";

        // Act & Assert
        var act = async () => await _cloudinaryService.DownloadFileAsync(invalidUrl);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid file URL format*");
    }

    [Fact]
    public async Task DownloadFileAsync_WhenHttpRequestFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var fileUrl = "https://cloudinary.com/test-file.pdf";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var act = async () => await _cloudinaryService.DownloadFileAsync(fileUrl);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to download file from URL: {fileUrl}");
    }

    [Fact]
    public async Task DownloadFileAsync_WhenResponseNotSuccessful_ShouldThrowHttpRequestException()
    {
        // Arrange
        var fileUrl = "https://cloudinary.com/test-file.pdf";

        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act & Assert
        var act = async () => await _cloudinaryService.DownloadFileAsync(fileUrl);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Failed to download file from URL: {fileUrl}");
    }

    [Theory]
    [InlineData("https://cloudinary.com/file.pdf")]
    [InlineData("http://cloudinary.com/file.jpg")]
    [InlineData("https://res.cloudinary.com/demo/raw/upload/sample.pdf")]
    public async Task DownloadFileAsync_WithValidUrls_ShouldMakeCorrectHttpRequest(string validUrl)
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent("content"u8.ToArray())
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        await _cloudinaryService.DownloadFileAsync(validUrl);

        // Assert
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == validUrl),
                ItExpr.IsAny<CancellationToken>());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}