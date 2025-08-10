using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;

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
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/");

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
		try
		{
			// This will call GET /api/Categories
			var response = await _httpClient.GetAsync("Categories");

			if (response.IsSuccessStatusCode)
			{
				var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategoryViewModel>>();
				return categories ?? new List<CategoryViewModel>();
			}

			_logger.LogWarning("Failed to get categories. Status: {StatusCode}", response.StatusCode);
			return new List<CategoryViewModel>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching categories");
			return new List<CategoryViewModel>();
		}
	}

	public async Task<CategoryViewModel?> CreateCategoryAsync(CategoryFormViewModel viewModel)
	{
		try
		{
			if (viewModel == null)
			{
				_logger.LogWarning("Attempted to create category with null viewModel");
				return null;
			}

			// This will call POST /api/Categories
			var response = await _httpClient.PostAsJsonAsync("Categories", viewModel);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
			}

			_logger.LogWarning("Failed to create category. Status: {StatusCode}", response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating category");
			return null;
		}
	}

	public async Task<CategoryFormViewModel?> GetCategoryByIdAsync(int id)
	{
		try
		{
			if (id <= 0)
			{
				_logger.LogWarning("Invalid category ID: {Id}", id);
				return null;
			}

			// This will call GET /api/Categories/{id}
			var response = await _httpClient.GetAsync($"Categories/{id}");
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<CategoryFormViewModel>();
			}

			_logger.LogWarning("Failed to get category by ID {Id}. Status: {StatusCode}", id, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching category by ID {Id}", id);
			return null;
		}
	}

	public async Task<CategoryViewModel?> UpdateCategoryAsync(CategoryFormViewModel viewModel)
	{
		try
		{
			if (viewModel == null)
			{
				_logger.LogWarning("Attempted to update category with null viewModel");
				return null;
			}

			// This will call PUT /api/Categories/{id}
			var response = await _httpClient.PutAsJsonAsync($"Categories/{viewModel.Id}", viewModel);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
			}

			_logger.LogWarning("Failed to update category {Id}. Status: {StatusCode}", viewModel.Id, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating category");
			return null;
		}
	}

	public async Task<ChangeCategoryStatusResultDto?> ChangeCategoryStatusAsync(int id)
	{
		try
		{
			if (id <= 0)
			{
				_logger.LogWarning("Invalid category ID for status change: {Id}", id);
				return null;
			}

			// This will call POST /api/Categories/{id}/ChangeStatus
			var response = await _httpClient.PostAsync($"Categories/{id}/ChangeStatus", null);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<ChangeCategoryStatusResultDto>();
			}

			_logger.LogWarning("Failed to change category status for ID {Id}. Status: {StatusCode}", id, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while changing category status for ID {Id}", id);
			return null;
		}
	}
}
