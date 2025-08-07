using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Text;
using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Services;

public class JobService
{
    private readonly HttpClient _httpClient;
    private readonly HttpContext _httpContext;

    public JobService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContext = httpContextAccessor.HttpContext;
        _httpClient.BaseAddress = new Uri("https://localhost:7000//api//Jobs//");

        var token = _httpContext?.User?.FindFirst("jwt")?.Value;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<PagedResultDto<JobViewModel>?> GetAllJobsAsync(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = null)
    {
        var queryParams = new List<string>();
        
        queryParams.Add($"pageNumber={pageNumber}");
        queryParams.Add($"pageSize={pageSize}");
        
        if (!string.IsNullOrEmpty(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        
        if (!string.IsNullOrEmpty(sortBy))
            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            
        if (!string.IsNullOrEmpty(sortDirection))
            queryParams.Add($"sortDirection={sortDirection}");

        var queryString = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"?{queryString}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PagedResultDto<JobViewModel>>();
        }

        return null;
    }

    public async Task<IEnumerable<JobViewModel>?> GetMyJobsAsync()
    {
        var response = await _httpClient.GetAsync("my-jobs");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<JobViewModel>>();
        }

        return null;
    }

    public async Task<JobViewModel?> GetJobByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{id}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<JobViewModel>();
        }

        return null;
    }

    public async Task<int?> CreateJobAsync(CreateJobViewModel viewModel)
    {
        var response = await _httpClient.PostAsJsonAsync("", viewModel);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (int.TryParse(content, out int jobId))
            {
                return jobId;
            }
        }

        return null;
    }

    public async Task<bool> UpdateJobAsync(UpdateJobViewModel viewModel)
    {
        var response = await _httpClient.PutAsJsonAsync($"{viewModel.Id}", viewModel);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{id}");
        return response.IsSuccessStatusCode;
    }
}