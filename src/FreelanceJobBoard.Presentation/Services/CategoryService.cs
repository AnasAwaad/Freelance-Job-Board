using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Presentation.Services;

public class CategoryService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext? _httpContext;
	private readonly ILogger<CategoryService> _logger;

	public CategoryService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<CategoryService> logger)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;
		
		// Fix URL - remove double slashes
		_httpClient.BaseAddress = new Uri("https://localhost:7000/api/Categories/");

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
			var response = await _httpClient.GetAsync("");

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

			var response = await _httpClient.PostAsJsonAsync("", viewModel);
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

			var response = await _httpClient.GetAsync($"{id}");
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

			var response = await _httpClient.PutAsJsonAsync($"{viewModel.Id}", viewModel);
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

			var response = await _httpClient.PostAsync($"{id}/ChangeStatus", null);
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
