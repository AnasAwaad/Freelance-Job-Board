using FreelanceJobBoard.Application.Features.User.DTOs;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.Services;

public class UserService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("http://localhost:5102/api/User/");
    }

    public async Task<GetProfileResponse?> GetCurrentUserProfileAsync()
    {
        try
        {
            // Get JWT token from claims
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                var jwtToken = context.User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
            }

            var response = await _httpClient.GetAsync("get-profile");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GetProfileResponse>();
            }

            _logger.LogWarning("Failed to get user profile. Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, await response.Content.ReadAsStringAsync());
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user profile");
            return null;
        }
    }

    public async Task<bool> UpdateFreelancerProfileAsync(UpdateFreelancerProfileRequest request)
    {
        try
        {
            // Get JWT token from claims
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                var jwtToken = context.User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
            }

            using var formData = new MultipartFormDataContent();
            
            if (!string.IsNullOrEmpty(request.FullName))
                formData.Add(new StringContent(request.FullName), "FullName");
            
            if (!string.IsNullOrEmpty(request.Bio))
                formData.Add(new StringContent(request.Bio), "Bio");
            
            if (!string.IsNullOrEmpty(request.Description))
                formData.Add(new StringContent(request.Description), "Description");
            
            if (request.HourlyRate.HasValue)
                formData.Add(new StringContent(request.HourlyRate.Value.ToString()), "HourlyRate");
            
            if (!string.IsNullOrEmpty(request.AvailabilityStatus))
                formData.Add(new StringContent(request.AvailabilityStatus), "AvailabilityStatus");

            if (request.ProfileImageFile != null)
            {
                var fileContent = new StreamContent(request.ProfileImageFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ProfileImageFile.ContentType);
                formData.Add(fileContent, "ProfileImageFile", request.ProfileImageFile.FileName);
            }

            var response = await _httpClient.PutAsync("update-freelancer-profile", formData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating freelancer profile");
            return false;
        }
    }

    public async Task<bool> UpdateClientProfileAsync(UpdateClientProfileRequest request)
    {
        try
        {
            // Get JWT token from claims
            var context = _httpContextAccessor.HttpContext;
            if (context?.User?.Identity?.IsAuthenticated == true)
            {
                var jwtToken = context.User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                }
            }

            using var formData = new MultipartFormDataContent();
            
            if (!string.IsNullOrEmpty(request.FullName))
                formData.Add(new StringContent(request.FullName), "FullName");
            
            if (!string.IsNullOrEmpty(request.CompanyName))
                formData.Add(new StringContent(request.CompanyName), "CompanyName");
            
            if (!string.IsNullOrEmpty(request.CompanyDescription))
                formData.Add(new StringContent(request.CompanyDescription), "CompanyDescription");
            
            if (!string.IsNullOrEmpty(request.CompanyLogoUrl))
                formData.Add(new StringContent(request.CompanyLogoUrl), "CompanyLogoUrl");
            
            if (!string.IsNullOrEmpty(request.CompanyWebsiteUrl))
                formData.Add(new StringContent(request.CompanyWebsiteUrl), "CompanyWebsiteUrl");
            
            if (!string.IsNullOrEmpty(request.CompanyIndustry))
                formData.Add(new StringContent(request.CompanyIndustry), "CompanyIndustry");

            if (request.ProfileImageFile != null)
            {
                var fileContent = new StreamContent(request.ProfileImageFile.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ProfileImageFile.ContentType);
                formData.Add(fileContent, "ProfileImageFile", request.ProfileImageFile.FileName);
            }

            var response = await _httpClient.PutAsync("update-client-profile", formData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating client profile");
            return false;
        }
    }
}

// Request DTOs for the UserService
public class UpdateFreelancerProfileRequest
{
    public string? FullName { get; set; }
    public IFormFile? ProfileImageFile { get; set; }
    public string? Bio { get; set; }
    public string? Description { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? AvailabilityStatus { get; set; }
}

public class UpdateClientProfileRequest
{
    public string? FullName { get; set; }
    public IFormFile? ProfileImageFile { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyDescription { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? CompanyWebsiteUrl { get; set; }
    public string? CompanyIndustry { get; set; }
}