using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

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
		_httpClient.BaseAddress = new Uri("https://localhost:7000/api/");

		var token = _httpContext?.User?.FindFirst("jwt")?.Value;

		_httpClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
	}

	public async Task<PagedResultDto<JobViewModel>?> GetAllJobsAsync(int pageNumber = 1, int pageSize = 10, string? search = null, string? sortBy = null, int? category = null, int? skill = null, string? sortDirection = null)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			_logger.LogInformation("?? Fetching jobs | User={UserId}, Session={SessionId}, Page={PageNumber}, Size={PageSize}, Search='{Search}', Sort={SortBy}:{SortDirection}, Category={Category}, Skill={Skill}",
				userId, sessionId, pageNumber, pageSize, search ?? "none", sortBy ?? "none", sortDirection ?? "none", category?.ToString() ?? "none", skill?.ToString() ?? "none");

			var queryParams = new List<string>();

			queryParams.Add($"pageNumber={pageNumber}");
			queryParams.Add($"pageSize={pageSize}");

			if (!string.IsNullOrEmpty(search))
				queryParams.Add($"search={Uri.EscapeDataString(search)}");

			if (!string.IsNullOrEmpty(sortBy))
				queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

			if (!string.IsNullOrEmpty(sortDirection))
				queryParams.Add($"sortDirection={sortDirection}");

			if (category.HasValue)
				queryParams.Add($"category={category.Value}");

			if (skill.HasValue)
				queryParams.Add($"skill={skill.Value}");

			var queryString = string.Join("&", queryParams);
			var apiUrl = $"Jobs?{queryString}";

			_logger.LogDebug("?? Making API request | URL='{ApiUrl}', BaseAddress='{BaseAddress}'", apiUrl, _httpClient.BaseAddress);

			// Log request headers
			LogRequestHeaders("GET_ALL_JOBS");

			// This will call GET /api/Jobs?queryString
			var response = await _httpClient.GetAsync(apiUrl);

			// Log response details
			_logger.LogDebug("?? API Response | Status={StatusCode} {ReasonPhrase}, ContentType='{ContentType}', ContentLength={ContentLength}",
				(int)response.StatusCode, response.StatusCode, response.ReasonPhrase,
				response.Content.Headers.ContentType?.ToString() ?? "unknown",
				response.Content.Headers.ContentLength ?? 0);

			if (response.IsSuccessStatusCode)
			{
				// API returns PagedResult<JobDto>, we need PagedResultDto<JobViewModel>
				var apiResponse = await response.Content.ReadFromJsonAsync<ApiPagedResult<ApiJobDto>>();
				if (apiResponse != null)
				{
					var viewModels = apiResponse.Items.Select(MapJobDtoToViewModel);
					var result = new PagedResultDto<JobViewModel>
					{
						Items = viewModels,
						TotalCount = apiResponse.TotalCount,
						PageNumber = apiResponse.PageNumber,
						PageSize = apiResponse.PageSize,
						TotalPages = apiResponse.TotalPages,
						HasNextPage = apiResponse.HasNextPage,
						HasPreviousPage = apiResponse.HasPreviousPage
					};

					stopwatch.Stop();
					_logger.LogInformation("? Jobs fetched successfully! Count={JobCount}/{TotalCount}, Pages={PageNumber}/{TotalPages}, User={UserId}, Duration={ElapsedMs}ms",
						result.Items?.Count() ?? 0, result.TotalCount, result.PageNumber, result.TotalPages, userId, stopwatch.ElapsedMilliseconds);

					// Log performance metrics
					if (stopwatch.ElapsedMilliseconds > 10000)
					{
						_logger.LogWarning("?? Slow API response | Operation=GetAllJobs, Duration={ElapsedMs}ms, User={UserId}",
							stopwatch.ElapsedMilliseconds, userId);
					}

					return result;
				}
			}

			stopwatch.Stop();
			_logger.LogWarning("?? Failed to get jobs | User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
				userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while fetching jobs | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message);
			return null;
		}
		catch (TaskCanceledException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "? Request timeout while fetching jobs | User={UserId}, Duration={ElapsedMs}ms",
				userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while fetching jobs | User={UserId}, Duration={ElapsedMs}ms, Error={ErrorMessage}",
				userId, stopwatch.ElapsedMilliseconds, ex.Message);
			return null;
		}
	}

	public async Task<IEnumerable<JobViewModel>?> GetMyJobsAsync()
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			// Check if user is a client or freelancer
			var isClient = _httpContext?.User?.IsInRole("Client") ?? false;
			var isFreelancer = _httpContext?.User?.IsInRole("Freelancer") ?? false;

			_logger.LogInformation("?? Fetching user-specific jobs | User={UserId}, Session={SessionId}, IsClient={IsClient}, IsFreelancer={IsFreelancer}",
				userId, sessionId, isClient, isFreelancer);

			string endpoint;
			string userType;
			if (isClient)
			{
				endpoint = "Jobs/my-jobs";
				userType = "Client";
			}
			else if (isFreelancer)
			{
				endpoint = "Jobs/my-freelancer-jobs";
				userType = "Freelancer";
			}
			else
			{
				stopwatch.Stop();
				_logger.LogWarning("?? User has no valid role | User={UserId}, Roles={Roles}, Duration={ElapsedMs}ms",
					userId, string.Join(",", _httpContext?.User?.Claims?.Where(c => c.Type.Contains("role"))?.Select(c => c.Value) ?? new[] { "none" }),
					stopwatch.ElapsedMilliseconds);
				return null;
			}

			_logger.LogDebug("?? Making user-specific API request | User={UserId}, UserType={UserType}, Endpoint='{Endpoint}'",
				userId, userType, endpoint);

			LogRequestHeaders($"GET_MY_JOBS_{userType.ToUpperInvariant()}");

			var response = await _httpClient.GetAsync(endpoint);

			_logger.LogDebug("?? API Response | Status={StatusCode} {ReasonPhrase}, UserType={UserType}",
				(int)response.StatusCode, response.StatusCode, response.ReasonPhrase, userType);

			if (response.IsSuccessStatusCode)
			{
				var apiJobs = await response.Content.ReadFromJsonAsync<IEnumerable<ApiJobDto>>();
				if (apiJobs != null)
				{
					var result = apiJobs.Select(MapJobDtoToViewModel);

					stopwatch.Stop();
					_logger.LogInformation("? User jobs fetched successfully! Count={JobCount}, UserType={UserType}, User={UserId}, Duration={ElapsedMs}ms",
						result.Count(), userType, userId, stopwatch.ElapsedMilliseconds);

					// Log job status breakdown
					var statusBreakdown = result.GroupBy(j => j.Status).ToDictionary(g => g.Key, g => g.Count());
					_logger.LogDebug("?? Job Status Breakdown | User={UserId}, StatusBreakdown={@StatusBreakdown}",
						userId, statusBreakdown);

					return result;
				}
			}

			stopwatch.Stop();
			_logger.LogWarning("?? Failed to get user jobs | User={UserId}, UserType={UserType}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
				userId, userType, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while fetching user jobs | User={UserId}, Duration={ElapsedMs}ms",
				userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while fetching user jobs | User={UserId}, Duration={ElapsedMs}ms",
				userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<JobViewModel?> GetJobByIdAsync(int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			_logger.LogInformation("?? Fetching job details | JobId={JobId}, User={UserId}, Session={SessionId}", id, userId, sessionId);

			LogRequestHeaders("GET_JOB_BY_ID");

			var response = await _httpClient.GetAsync($"Jobs/{id}");

			_logger.LogDebug("?? API Response | JobId={JobId}, Status={StatusCode} {ReasonPhrase}",
				id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				var apiJob = await response.Content.ReadFromJsonAsync<ApiJobDetailsDto>();
				if (apiJob != null)
				{
					var result = MapJobDetailsDtoToViewModel(apiJob);

					stopwatch.Stop();
					_logger.LogInformation("? Job details fetched successfully! JobId={JobId}, Title='{JobTitle}', Status='{Status}', User={UserId}, Duration={ElapsedMs}ms",
						id, result.Title, result.Status, userId, stopwatch.ElapsedMilliseconds);

					// Log job insights
					_logger.LogDebug("?? Job Budget | JobId={JobId}, Min=${BudgetMin}, Max=${BudgetMax}, Deadline={Deadline}",
						id, result.BudgetMin, result.BudgetMax, result.Deadline);

					return result;
				}
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("? Job not found | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("?? Failed to get job details | JobId={JobId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
					id, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while fetching job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while fetching job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<int?> CreateJobAsync(CreateJobViewModel viewModel)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)!.Value ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			_logger.LogInformation("?? Creating job | Title='{JobTitle}', User={UserId}, Session={SessionId}", viewModel.Title, userId, sessionId);
			_logger.LogInformation("?? Job Details | Budget=${BudgetMin}-${BudgetMax}, Deadline={Deadline}, Skills={SkillCount}, Categories={CategoryCount}",
				viewModel.BudgetMin, viewModel.BudgetMax, viewModel.Deadline,
				viewModel.SkillIds?.Count() ?? 0, viewModel.CategoryIds?.Count() ?? 0);

			LogRequestHeaders("CREATE_JOB");
			_logger.LogDebug("?? Job creation data: {@JobData}", new
			{
				viewModel.Title,
				viewModel.BudgetMin,
				viewModel.BudgetMax,
				viewModel.Deadline,
				viewModel.Description?.Length,
				SkillCount = viewModel.SkillIds?.Count() ?? 0,
				CategoryCount = viewModel.CategoryIds?.Count() ?? 0
			});

			var response = await _httpClient.PostAsJsonAsync("Jobs", viewModel);

			_logger.LogDebug("?? API Response | Status={StatusCode} {ReasonPhrase}, ContentLength={ContentLength}",
				(int)response.StatusCode, response.StatusCode, response.ReasonPhrase,
				response.Content.Headers.ContentLength ?? 0);

			if (response.IsSuccessStatusCode)
			{
				try
				{
					var jobIdFromJson = await response.Content.ReadFromJsonAsync<int>();

					stopwatch.Stop();
					_logger.LogInformation("? Job created successfully! JobId={JobId}, Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
						jobIdFromJson, viewModel.Title, userId, stopwatch.ElapsedMilliseconds);

					return jobIdFromJson;
				}
				catch (Exception parseEx)
				{
					_logger.LogWarning(parseEx, "?? Failed to parse job ID from JSON response | User={UserId}", userId);

					// Fallback: try parsing as plain text
					try
					{
						var content = await response.Content.ReadAsStringAsync();
						if (int.TryParse(content, out int jobId))
						{
							stopwatch.Stop();
							_logger.LogInformation("? Job created successfully (text parsed)! JobId={JobId}, Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
								jobId, viewModel.Title, userId, stopwatch.ElapsedMilliseconds);
							return jobId;
						}
					}
					catch (Exception textParseEx)
					{
						_logger.LogWarning(textParseEx, "?? Failed to parse job ID from text response | User={UserId}", userId);
					}

					// Last resort: extract from location header
					if (response.Headers.Location != null)
					{
						var locationPath = response.Headers.Location.AbsolutePath;
						var segments = locationPath.Split('/');
						if (segments.Length > 0 && int.TryParse(segments.Last(), out int idFromLocation))
						{
							stopwatch.Stop();
							_logger.LogInformation("? Job created successfully (location header)! JobId={JobId}, Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
								idFromLocation, viewModel.Title, userId, stopwatch.ElapsedMilliseconds);
							return idFromLocation;
						}
					}
				}
			}

			stopwatch.Stop();
			_logger.LogWarning("?? Failed to create job | Title='{JobTitle}', User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
				viewModel.Title, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while creating job | Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
				viewModel.Title, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while creating job | Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
				viewModel.Title, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<bool> UpdateJobAsync(UpdateJobViewModel viewModel)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			_logger.LogInformation("?? Updating job | JobId={JobId}, Title='{JobTitle}', User={UserId}, Session={SessionId}",
				viewModel.Id, viewModel.Title, userId, sessionId);
			_logger.LogDebug("?? Updated Job Details | Budget=${BudgetMin}-${BudgetMax}, Deadline={Deadline}, Skills={SkillCount}, Categories={CategoryCount}",
				viewModel.BudgetMin, viewModel.BudgetMax, viewModel.Deadline,
				viewModel.SkillIds?.Count() ?? 0, viewModel.CategoryIds?.Count() ?? 0);

			LogRequestHeaders("UPDATE_JOB");

			var response = await _httpClient.PutAsJsonAsync($"Jobs/{viewModel.Id}", viewModel);

			_logger.LogDebug("?? API Response | JobId={JobId}, Status={StatusCode} {ReasonPhrase}",
				viewModel.Id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				stopwatch.Stop();
				_logger.LogInformation("? Job updated successfully! JobId={JobId}, Title='{JobTitle}', User={UserId}, Duration={ElapsedMs}ms",
					viewModel.Id, viewModel.Title, userId, stopwatch.ElapsedMilliseconds);
				return true;
			}

			stopwatch.Stop();
			_logger.LogWarning("?? Failed to update job | JobId={JobId}, Title='{JobTitle}', User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
				viewModel.Id, viewModel.Title, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			return false;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while updating job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				viewModel.Id, userId, stopwatch.ElapsedMilliseconds);
			return false;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while updating job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				viewModel.Id, userId, stopwatch.ElapsedMilliseconds);
			return false;
		}
	}

	public async Task<bool> DeleteJobAsync(int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";

		try
		{
			_logger.LogInformation("??? Deleting job | JobId={JobId}, User={UserId}, Session={SessionId}", id, userId, sessionId);

			LogRequestHeaders("DELETE_JOB");

			var response = await _httpClient.DeleteAsync($"Jobs/{id}");

			_logger.LogDebug("?? API Response | JobId={JobId}, Status={StatusCode} {ReasonPhrase}",
				id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				stopwatch.Stop();
				_logger.LogInformation("? Job deleted successfully! JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
					id, userId, stopwatch.ElapsedMilliseconds);
				return true;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("? Job not found for deletion | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("?? Failed to delete job | JobId={JobId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms",
					id, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return false;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? HTTP error while deleting job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				id, userId, stopwatch.ElapsedMilliseconds);
			return false;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "?? Unexpected error while deleting job | JobId={JobId}, User={UserId}, Duration={ElapsedMs}ms",
				id, userId, stopwatch.ElapsedMilliseconds);
			return false;
		}
	}

	private void LogRequestHeaders(string operation)
	{
		try
		{
			var headers = new Dictionary<string, string>();

			// Log important headers
			foreach (var header in _httpClient.DefaultRequestHeaders)
			{
				if (IsImportantHeader(header.Key))
				{
					headers[header.Key] = GetSafeHeaderValue(string.Join(", ", header.Value));
				}
			}

			if (headers.Any())
			{
				_logger.LogDebug("?? {Operation} - Request Headers: {@Headers}", operation, headers);
			}

			// Log authorization status
			var hasAuth = _httpClient.DefaultRequestHeaders.Authorization != null;
			_logger.LogDebug("?? Authorization Status | Operation={Operation}, HasAuth={HasAuth}, Scheme={Scheme}",
				operation, hasAuth, _httpClient.DefaultRequestHeaders.Authorization?.Scheme ?? "none");

			// Log user context
			var userClaims = _httpContext?.User?.Claims?.Where(c => !c.Type.Contains("nbf") && !c.Type.Contains("exp") && !c.Type.Contains("iat"))
				.ToDictionary(c => c.Type.Split('/').Last(), c => c.Value);

			if (userClaims?.Any() == true)
			{
				_logger.LogDebug("?? User Context | Operation={Operation}, Claims={@Claims}", operation, userClaims);
			}
		}
		catch (Exception ex)
		{
			_logger.LogDebug(ex, "Failed to log request headers for operation {Operation}", operation);
		}
	}

	private static bool IsImportantHeader(string headerName)
	{
		var important = new[]
		{
			"Authorization", "Accept", "Content-Type", "User-Agent"
		};
		return important.Contains(headerName, StringComparer.OrdinalIgnoreCase);
	}

	private static string GetSafeHeaderValue(string headerValue)
	{
		if (string.IsNullOrEmpty(headerValue))
			return "";

		// Redact sensitive headers
		if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			return "Bearer [REDACTED]";

		return headerValue.Length > 100 ? headerValue[..100] + "[TRUNCATED]" : headerValue;
	}

	// Mapping methods remain the same...
	private static JobViewModel MapJobDtoToViewModel(ApiJobDto dto)
	{
		return new JobViewModel
		{
			Id = dto.Id,
			ClientId = dto.ClientId,
			ClientName = dto.ClientName,
			ClientProfileImageUrl = dto.ClientProfileImageUrl,
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
			CreatedOn = dto.CreatedOn,
			Categories = dto.Categories?.Select(c => new CategoryViewModel
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				IsActive = true
			}).ToList() ?? new List<CategoryViewModel>(),
			Skills = dto.Skills?.Select(s => new SkillViewModel
			{
				Id = s.Id,
				Name = s.Name,
				IsActive = true,
				CreatedOn = dto.CreatedOn
			}).ToList() ?? new List<SkillViewModel>()
		};
	}

	private static JobViewModel MapJobDetailsDtoToViewModel(ApiJobDetailsDto dto)
	{
		return new JobViewModel
		{
			Id = dto.Id,
			ClientId = dto.Client?.Id,
			Title = dto.Title,
			Description = dto.Description,
			BudgetMin = dto.BudgetMin,
			BudgetMax = dto.BudgetMax,
			Deadline = dto.Deadline,
			Status = dto.Status,
			RequiredSkills = null,
			Tags = null,
			ViewsCount = 0,
			IsApproved = dto.Status != "Pending",
			ApprovedBy = null,
			Categories = new List<CategoryViewModel>(),
			Skills = new List<SkillViewModel>()
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

	public async Task<int?> GetNumberOfJobsAsync()
	{
		var response = await _httpClient.GetAsync($"Jobs/total-jobs");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<int>();

		return null;
	}

	public async Task<IEnumerable<JobSearchResult>> SearchJobsAsync(string query)
	{
		var response = await _httpClient.GetAsync($"Jobs/search?query={query}");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<IEnumerable<JobSearchResult>>();

		return new List<JobSearchResult>();
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
	public string? ClientName { get; set; }
	public string? ClientProfileImageUrl { get; set; }
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
	public DateTime CreatedOn { get; set; }
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


