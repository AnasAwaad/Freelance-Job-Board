using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Diagnostics;

namespace FreelanceJobBoard.Presentation.Services;

public class CategoryService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext? _httpContext;
	private readonly ILogger<CategoryService> _logger;

	public CategoryService(HttpClient httpClient,
		IHttpContextAccessor httpContextAccessor,
		ILogger<CategoryService> logger,
		IConfiguration configuration)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;

		// Set base address to API root - don't include Categories path here
		_httpClient.BaseAddress = new Uri("https://localhost:7000/api/");

		// Set authorization header if user is authenticated
		var token = _httpContext?.User?.FindFirst("jwt")?.Value;
		if (!string.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}
	}

	public async Task<IEnumerable<CategoryViewModel>> GetAllCategoriesAsync()
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			_logger.LogInformation("📂 Fetching all categories | User={UserId}, Session={SessionId}", userId, sessionId);

			LogRequestHeaders("GET_ALL_CATEGORIES");

			// This will call GET /api/Categories
			var response = await _httpClient.GetAsync("Categories");

			_logger.LogDebug("📥 API Response | Status={StatusCode} {ReasonPhrase}, ContentType='{ContentType}'", 
				(int)response.StatusCode, response.StatusCode, response.ReasonPhrase, 
				response.Content.Headers.ContentType?.ToString() ?? "unknown");

			if (response.IsSuccessStatusCode)
			{
				var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryViewModel>>();
				var categoryList = categories?.ToList() ?? new List<CategoryViewModel>();
				
				stopwatch.Stop();
				_logger.LogInformation("✅ Categories fetched successfully! Count={CategoryCount}, User={UserId}, Duration={ElapsedMs}ms", 
					categoryList.Count, userId, stopwatch.ElapsedMilliseconds);

				// Log category breakdown
				if (categoryList.Any())
				{
					var activeCount = categoryList.Count(c => c.IsActive);
					_logger.LogDebug("📊 Category Stats | Active={ActiveCount}, Inactive={InactiveCount}, User={UserId}", 
						activeCount, categoryList.Count - activeCount, userId);
				}

				// Log performance warning
				if (stopwatch.ElapsedMilliseconds > 1000)
				{
					_logger.LogWarning("🐌 Slow category fetch | Duration={ElapsedMs}ms, User={UserId}", 
						stopwatch.ElapsedMilliseconds, userId);
				}

				return categoryList;
			}

			stopwatch.Stop();
			_logger.LogWarning("⚠️ Failed to get categories | User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
				userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			return new List<CategoryViewModel>();
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while fetching categories | User={UserId}, Duration={ElapsedMs}ms", 
				userId, stopwatch.ElapsedMilliseconds);
			return new List<CategoryViewModel>();
		}
		catch (TaskCanceledException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "⏰ Request timeout while fetching categories | User={UserId}, Duration={ElapsedMs}ms", 
				userId, stopwatch.ElapsedMilliseconds);
			return new List<CategoryViewModel>();
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while fetching categories | User={UserId}, Duration={ElapsedMs}ms", 
				userId, stopwatch.ElapsedMilliseconds);
			return new List<CategoryViewModel>();
		}
	}

	public async Task<CategoryViewModel?> CreateCategoryAsync(CategoryFormViewModel viewModel)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			if (viewModel == null)
			{
				stopwatch.Stop();
				_logger.LogWarning("⚠️ Attempted to create category with null viewModel | User={UserId}, Duration={ElapsedMs}ms", 
					userId, stopwatch.ElapsedMilliseconds);
				return null;
			}

			_logger.LogInformation("🆕 Creating category | Name='{CategoryName}', User={UserId}, Session={SessionId}", 
				viewModel.Name, userId, sessionId);
			_logger.LogDebug("📝 Category Details | Name='{Name}', Description='{Description}', IsActive={IsActive}", 
				viewModel.Name, viewModel.Description, viewModel.IsActive);

			LogRequestHeaders("CREATE_CATEGORY");

			// This will call POST /api/Categories
			var response = await _httpClient.PostAsJsonAsync("Categories", viewModel);

			_logger.LogDebug("📥 API Response | Status={StatusCode} {ReasonPhrase}, ContentLength={ContentLength}", 
				(int)response.StatusCode, response.StatusCode, response.ReasonPhrase, 
				response.Content.Headers.ContentLength ?? 0);

			if (response.IsSuccessStatusCode)
			{
				var createdCategory = await response.Content.ReadFromJsonAsync<CategoryViewModel>();
				
				stopwatch.Stop();
				_logger.LogInformation("✅ Category created successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
					createdCategory?.Id ?? 0, viewModel.Name, userId, stopwatch.ElapsedMilliseconds);

				return createdCategory;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
			{
				_logger.LogWarning("🔄 Category creation conflict | Name='{CategoryName}', User={UserId}, Status={StatusCode}, Duration={ElapsedMs}ms", 
					viewModel.Name, userId, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("⚠️ Failed to create category | Name='{CategoryName}', User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
					viewModel.Name, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while creating category | Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
				viewModel?.Name ?? "Unknown", userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while creating category | Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
				viewModel?.Name ?? "Unknown", userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<CategoryFormViewModel?> GetCategoryByIdAsync(int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			if (id <= 0)
			{
				stopwatch.Stop();
				_logger.LogWarning("⚠️ Invalid category ID | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
				return null;
			}

			_logger.LogInformation("🔍 Fetching category details | CategoryId={CategoryId}, User={UserId}, Session={SessionId}", 
				id, userId, sessionId);

			LogRequestHeaders("GET_CATEGORY_BY_ID");

			// This will call GET /api/Categories/{id}
			var response = await _httpClient.GetAsync($"Categories/{id}");

			_logger.LogDebug("📥 API Response | CategoryId={CategoryId}, Status={StatusCode} {ReasonPhrase}", 
				id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				var category = await response.Content.ReadFromJsonAsync<CategoryFormViewModel>();
				
				stopwatch.Stop();
				_logger.LogInformation("✅ Category details fetched successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
					id, category?.Name ?? "Unknown", userId, stopwatch.ElapsedMilliseconds);

				// Log category info
				if (category != null)
				{
					_logger.LogDebug("📋 Category Info | Name='{Name}', IsActive={IsActive}, Description='{Description}'", 
						category.Name, category.IsActive, category.Description?.Length > 50 ? category.Description[..50] + "..." : category.Description ?? "None");
				}

				return category;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("❌ Category not found | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("⚠️ Failed to get category details | CategoryId={CategoryId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
					id, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while fetching category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while fetching category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<CategoryViewModel?> UpdateCategoryAsync(CategoryFormViewModel viewModel)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			if (viewModel == null)
			{
				stopwatch.Stop();
				_logger.LogWarning("⚠️ Attempted to update category with null viewModel | User={UserId}, Duration={ElapsedMs}ms", 
					userId, stopwatch.ElapsedMilliseconds);
				return null;
			}

			_logger.LogInformation("🔄 Updating category | CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Session={SessionId}", 
				viewModel.Id, viewModel.Name, userId, sessionId);
			_logger.LogDebug("📝 Update Details | Name='{Name}', Description='{Description}', IsActive={IsActive}", 
				viewModel.Name, viewModel.Description, viewModel.IsActive);

			LogRequestHeaders("UPDATE_CATEGORY");

			// This will call PUT /api/Categories/{id}
			var response = await _httpClient.PutAsJsonAsync($"Categories/{viewModel.Id}", viewModel);

			_logger.LogDebug("📥 API Response | CategoryId={CategoryId}, Status={StatusCode} {ReasonPhrase}", 
				viewModel.Id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				var updatedCategory = await response.Content.ReadFromJsonAsync<CategoryViewModel>();
				
				stopwatch.Stop();
				_logger.LogInformation("✅ Category updated successfully! CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
					viewModel.Id, viewModel.Name, userId, stopwatch.ElapsedMilliseconds);

				return updatedCategory;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("❌ Category not found for update | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					viewModel.Id, userId, stopwatch.ElapsedMilliseconds);
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
			{
				_logger.LogWarning("🔄 Category update conflict | CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Duration={ElapsedMs}ms", 
					viewModel.Id, viewModel.Name, userId, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("⚠️ Failed to update category | CategoryId={CategoryId}, Name='{CategoryName}', User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
					viewModel.Id, viewModel.Name, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while updating category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				viewModel?.Id ?? 0, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while updating category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				viewModel?.Id ?? 0, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<ChangeCategoryStatusResultDto?> ChangeCategoryStatusAsync(int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			if (id <= 0)
			{
				stopwatch.Stop();
				_logger.LogWarning("⚠️ Invalid category ID for status change | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
				return null;
			}

			_logger.LogInformation("🔄 Changing category status | CategoryId={CategoryId}, User={UserId}, Session={SessionId}", 
				id, userId, sessionId);

			LogRequestHeaders("CHANGE_CATEGORY_STATUS");

			// This will call POST /api/Categories/{id}/ChangeStatus
			var response = await _httpClient.PostAsync($"Categories/{id}/ChangeStatus", null);

			_logger.LogDebug("📥 API Response | CategoryId={CategoryId}, Status={StatusCode} {ReasonPhrase}", 
				id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<ChangeCategoryStatusResultDto>();
				
				stopwatch.Stop();
				_logger.LogInformation("✅ Category status changed successfully! CategoryId={CategoryId}, NewStatus={IsActive}, User={UserId}, Duration={ElapsedMs}ms", 
					id, result?.NewStatus ?? false, userId, stopwatch.ElapsedMilliseconds);

				// Log status change details
				if (result != null)
				{
					_logger.LogDebug("📊 Status Change | CategoryId={CategoryId}, NewStatus={NewStatus}, Success={Success}", 
						id, result.NewStatus, result.Success);
				}

				return result;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("❌ Category not found for status change | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
			{
				_logger.LogWarning("🔄 Category status change conflict | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("⚠️ Failed to change category status | CategoryId={CategoryId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
					id, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return null;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while changing category status | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while changing category status | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				id, userId, stopwatch.ElapsedMilliseconds);
			return null;
		}
	}

	public async Task<IEnumerable<PublicCategoryViewModel>?> GetTopCategories(int numOfCategories)
	{
		var response = await _httpClient.GetAsync($"Categories/top/{numOfCategories}");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<IEnumerable<PublicCategoryViewModel>>();

		return new List<PublicCategoryViewModel>();
	}

	public async Task<bool> DeleteCategoryAsync(int id)
	{
		var stopwatch = Stopwatch.StartNew();
		var userId = _httpContext?.User?.Identity?.Name ?? "Anonymous";
		var sessionId = _httpContext?.Session?.Id ?? "NoSession";
		
		try
		{
			if (id <= 0)
			{
				stopwatch.Stop();
				_logger.LogWarning("⚠️ Invalid category ID for deletion | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
				return false;
			}

			_logger.LogInformation("🗑️ Deleting category | CategoryId={CategoryId}, User={UserId}, Session={SessionId}", 
				id, userId, sessionId);

			LogRequestHeaders("DELETE_CATEGORY");

			// This will call DELETE /api/Categories/{id}
			var response = await _httpClient.DeleteAsync($"Categories/{id}");

			_logger.LogDebug("📥 API Response | CategoryId={CategoryId}, Status={StatusCode} {ReasonPhrase}", 
				id, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				stopwatch.Stop();
				_logger.LogInformation("✅ Category deleted successfully! CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);

				return true;
			}

			stopwatch.Stop();
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				_logger.LogWarning("❌ Category not found for deletion | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
					id, userId, stopwatch.ElapsedMilliseconds);
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				_logger.LogWarning("⚠️ Cannot delete category | CategoryId={CategoryId}, User={UserId}, Error={Error}, Duration={ElapsedMs}ms", 
					id, userId, errorContent, stopwatch.ElapsedMilliseconds);
			}
			else
			{
				_logger.LogWarning("⚠️ Failed to delete category | CategoryId={CategoryId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
					id, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
			}
			return false;
		}
		catch (HttpRequestException ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🌐 HTTP error while deleting category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
				id, userId, stopwatch.ElapsedMilliseconds);
			return false;
		}
		catch (Exception ex)
		{
			stopwatch.Stop();
			_logger.LogError(ex, "🔥 Unexpected error while deleting category | CategoryId={CategoryId}, User={UserId}, Duration={ElapsedMs}ms", 
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
				_logger.LogDebug("📋 {Operation} - Request Headers: {@Headers}", operation, headers);
			}

			// Log authorization status
			var hasAuth = _httpClient.DefaultRequestHeaders.Authorization != null;
			_logger.LogDebug("🔑 Authorization Status | Operation={Operation}, HasAuth={HasAuth}, Scheme={Scheme}", 
				operation, hasAuth, _httpClient.DefaultRequestHeaders.Authorization?.Scheme ?? "none");

			// Log user context
			var userClaims = _httpContext?.User?.Claims?.Where(c => !c.Type.Contains("nbf") && !c.Type.Contains("exp") && !c.Type.Contains("iat"))
				.ToDictionary(c => c.Type.Split('/').Last(), c => c.Value);
			
			if (userClaims?.Any() == true)
			{
				_logger.LogDebug("👤 User Context | Operation={Operation}, Claims={@Claims}", operation, userClaims);
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
}
