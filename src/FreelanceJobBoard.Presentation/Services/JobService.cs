using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class JobService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext _httpContext;
	private readonly ILogger<JobService> _logger;

	public JobService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<JobService> logger)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;
		// Set base address to API root - don't include Jobs path here
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/");

		var token = _httpContext?.User?.FindFirst("jwt")?.Value;

		_httpClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
	}

	public async Task<PagedResultDto<JobViewModel>?> GetAllJobsAsync(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, string? sortDirection = null)
	{
		try
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
			// This will call GET /api/Jobs?queryString
			var response = await _httpClient.GetAsync($"Jobs?{queryString}");

			if (response.IsSuccessStatusCode)
			{
				// API returns PagedResult<JobDto>, we need PagedResultDto<JobViewModel>
				var apiResponse = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiJobDto>>();
				if (apiResponse != null)
				{
					var viewModels = apiResponse.Items.Select(MapJobDtoToViewModel);
					return new PagedResultDto<JobViewModel>
					{
						Items = viewModels,
						TotalCount = apiResponse.TotalCount,
						PageNumber = apiResponse.PageNumber,
						PageSize = apiResponse.PageSize,
						TotalPages = apiResponse.TotalPages,
						HasNextPage = apiResponse.HasNextPage,
						HasPreviousPage = apiResponse.HasPreviousPage
					};
				}
			}

			_logger.LogWarning("Failed to get jobs. Status: {StatusCode}", response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching jobs");
			return null;
		}
	}

	public async Task<IEnumerable<JobViewModel>?> GetMyJobsAsync()
	{
		try
		{
			// This will call GET /api/Jobs/my-jobs
			var response = await _httpClient.GetAsync("Jobs/my-jobs");

			if (response.IsSuccessStatusCode)
			{
				// API returns IEnumerable<JobDto>, we need IEnumerable<JobViewModel>
				var apiJobs = await response.Content.ReadFromJsonAsync<IEnumerable<ApiJobDto>>();
				if (apiJobs != null)
				{
					return apiJobs.Select(MapJobDtoToViewModel);
				}
			}

			_logger.LogWarning("Failed to get my jobs. Status: {StatusCode}", response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching my jobs");
			return null;
		}
	}

	public async Task<JobViewModel?> GetJobByIdAsync(int id)
	{
		try
		{
			// This will call GET /api/Jobs/{id}
			var response = await _httpClient.GetAsync($"Jobs/{id}");

			if (response.IsSuccessStatusCode)
			{
				// API returns JobDetailsDto, we need JobViewModel
				var apiJob = await response.Content.ReadFromJsonAsync<ApiJobDetailsDto>();
				if (apiJob != null)
				{
					return MapJobDetailsDtoToViewModel(apiJob);
				}
			}

			_logger.LogWarning("Failed to get job by ID {Id}. Status: {StatusCode}", id, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching job by ID {Id}", id);
			return null;
		}
	}

	public async Task<int?> CreateJobAsync(CreateJobViewModel viewModel)
	{
		try
		{
			// This will call POST /api/Jobs
			var response = await _httpClient.PostAsJsonAsync("Jobs", viewModel);

			if (response.IsSuccessStatusCode)
			{
				// Try to read the job ID from the response body
				// The API returns the job ID as JSON integer
				try
				{
					var jobIdFromJson = await response.Content.ReadFromJsonAsync<int>();
					_logger.LogInformation("Job created successfully with ID {JobId}", jobIdFromJson);
					return jobIdFromJson;
				}
				catch (Exception parseEx)
				{
					_logger.LogWarning(parseEx, "Failed to parse job ID from JSON response");

					// Fallback: try parsing as plain text
					try
					{
						var content = await response.Content.ReadAsStringAsync();
						if (int.TryParse(content, out int jobId))
						{
							_logger.LogInformation("Job created successfully with ID {JobId} (parsed as text)", jobId);
							return jobId;
						}
					}
					catch (Exception textParseEx)
					{
						_logger.LogWarning(textParseEx, "Failed to parse job ID from text response");
					}

					// Last resort: extract from location header
					if (response.Headers.Location != null)
					{
						var locationPath = response.Headers.Location.AbsolutePath;
						var segments = locationPath.Split('/');
						if (segments.Length > 0 && int.TryParse(segments.Last(), out int idFromLocation))
						{
							_logger.LogInformation("Job created successfully, ID extracted from location header: {JobId}", idFromLocation);
							return idFromLocation;
						}
					}
				}
			}

			_logger.LogWarning("Failed to create job. Status: {StatusCode}", response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating job");
			return null;
		}
	}

	public async Task<bool> UpdateJobAsync(UpdateJobViewModel viewModel)
	{
		try
		{
			// This will call PUT /api/Jobs/{id}
			var response = await _httpClient.PutAsJsonAsync($"Jobs/{viewModel.Id}", viewModel);

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Job updated successfully with ID {JobId}", viewModel.Id);
				return true;
			}

			_logger.LogWarning("Failed to update job {Id}. Status: {StatusCode}", viewModel.Id, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating job {Id}", viewModel.Id);
			return false;
		}
	}

	public async Task<bool> DeleteJobAsync(int id)
	{
		try
		{
			// This will call DELETE /api/Jobs/{id}
			var response = await _httpClient.DeleteAsync($"Jobs/{id}");

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Job deleted successfully with ID {JobId}", id);
				return true;
			}

			_logger.LogWarning("Failed to delete job {Id}. Status: {StatusCode}", id, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting job {Id}", id);
			return false;
		}
	}

	// Mapping methods to convert API DTOs to ViewModels
	private static JobViewModel MapJobDtoToViewModel(ApiJobDto dto)
	{
		return new JobViewModel
		{
			Id = dto.Id,
			ClientId = dto.ClientId,
			Title = dto.Title,
			Description = dto.Description,
			BudgetMin = dto.BudgetMin,
			BudgetMax = dto.BudgetMax,
			Deadline = dto.Deadline,
			Status = dto.Status,
			RequiredSkills = dto.RequiredSkills,
			Tags = dto.Tags,
			ViewsCount = dto.ViewsCount,
			IsApproved = dto.IsApproved,
			ApprovedBy = dto.ApprovedBy,
			Categories = dto.Categories?.Select(c => new CategoryViewModel
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				IsActive = true // Default value since not available in DTO
			}).ToList() ?? new List<CategoryViewModel>(),
			Skills = dto.Skills?.Select(s => new SkillViewModel
			{
				Id = s.Id,
				Name = s.Name,
				IsActive = true, // Default value since not available in DTO
				CreatedOn = DateTime.Now // Default value since not available in DTO
			}).ToList() ?? new List<SkillViewModel>()
		};
	}

	private static JobViewModel MapJobDetailsDtoToViewModel(ApiJobDetailsDto dto)
	{
		return new JobViewModel
		{
			Id = dto.Id,
			ClientId = dto.Client?.Id, // Assuming Client has Id property
			Title = dto.Title,
			Description = dto.Description,
			BudgetMin = dto.BudgetMin,
			BudgetMax = dto.BudgetMax,
			Deadline = dto.Deadline,
			Status = dto.Status,
			RequiredSkills = null, // Not available in JobDetailsDto
			Tags = null, // Not available in JobDetailsDto
			ViewsCount = 0, // Not available in JobDetailsDto
			IsApproved = dto.Status != "Pending", // Infer from status
			ApprovedBy = null, // Not available in JobDetailsDto
			Categories = new List<CategoryViewModel>(), // Not available in JobDetailsDto
			Skills = new List<SkillViewModel>() // Not available in JobDetailsDto
		};
	}

	public async Task<PublicJobDetailsViewModel?> GetPublicJobDeatils(int jobId)
	{
		var response = await _httpClient.GetAsync($"Jobs/details/{jobId}");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<PublicJobDetailsViewModel>();

		return null;
	}

	public async Task<IEnumerable<JobListViewModel>?> GetSimilarJobs(int jobId)
	{
		var response = await _httpClient.GetAsync($"Jobs/related-jobs/{jobId}");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<IEnumerable<JobListViewModel>>();

		return null;
	}
}

// API DTO classes (these should match what the API returns)
public class ApiPagedResult<T>
{
	public IEnumerable<T> Items { get; set; } = new List<T>();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
	public bool HasNextPage { get; set; }
	public bool HasPreviousPage { get; set; }
}

public class ApiJobDto
{
	public int Id { get; set; }
	public int? ClientId { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public DateTime Deadline { get; set; }
	public string Status { get; set; } = null!;
	public string? RequiredSkills { get; set; }
	public string? Tags { get; set; }
	public int ViewsCount { get; set; }
	public bool IsApproved { get; set; }
	public int? ApprovedBy { get; set; }
	public ICollection<ApiCategoryDto>? Categories { get; set; }
	public ICollection<ApiSkillDto>? Skills { get; set; }
}

public class ApiJobDetailsDto
{
	public int Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public decimal BudgetMin { get; set; }
	public decimal BudgetMax { get; set; }
	public string Status { get; set; } = null!;
	public DateTime Deadline { get; set; }
	public ApiClientDto? Client { get; set; }
	// Other properties from JobDetailsDto that we don't map for now
}

public class ApiCategoryDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
}

public class ApiSkillDto
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
}

public class ApiClientDto
{
	public int Id { get; set; }
	public string? FullName { get; set; }
}


