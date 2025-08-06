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

	public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
	{
		var response = await _httpClient.GetAsync("");

		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();
		}

		return null;
	}
}
