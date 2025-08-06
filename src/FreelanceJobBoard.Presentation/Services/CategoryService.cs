using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;

public class CategoryService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext _httpContext;

	public CategoryService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Categories/");

		var token = _httpContext?.User?.FindFirst("jwt")?.Value;

		_httpClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
	}

	public async Task<IEnumerable<CategoryViewModel>?> GetAllCategoriesAsync()
	{
		var response = await _httpClient.GetAsync("");

		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<IEnumerable<CategoryViewModel>>();
		}

		return null!;
	}


	public async Task<CategoryViewModel?> CreateCategoryAsync(CategoryFormViewModel viewModel)
	{
		var response = await _httpClient.PostAsJsonAsync("", viewModel);
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
		}
		return null!;
	}

	public async Task<CategoryFormViewModel?> GetCategoryByIdAsync(int id)
	{
		var response = await _httpClient.GetAsync($"{id}");
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<CategoryFormViewModel>();
		}
		return null!;
	}

	public async Task<CategoryViewModel?> UpdateCategoryAsync(CategoryFormViewModel viewModel)
	{
		var response = await _httpClient.PutAsJsonAsync($"{viewModel.Id}", viewModel);
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
		}
		return null!;
	}

	public async Task<ChangeCategoryStatusResultDto?> ChangeCategoryStatusAsync(int id)
	{
		var response = await _httpClient.PostAsync($"{id}/ChangeStatus", null);
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<ChangeCategoryStatusResultDto>();
		}
		throw new Exception("Failed to change category status");
	}
}
