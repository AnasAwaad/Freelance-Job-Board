using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class HomeService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<CategoryService> _logger;

	public HomeService(HttpClient httpClient,
		ILogger<CategoryService> logger,
		IConfiguration configuration)
	{
		_httpClient = httpClient;
		_logger = logger;

		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Jobs/");

	}

	public async Task<IEnumerable<RecentJobViewModel>?> GetRecentJobsAsync()
	{
		var response = await _httpClient.GetAsync("recent-jobs/6");

		if (response.IsSuccessStatusCode)
			return await response.Content.ReadFromJsonAsync<IEnumerable<RecentJobViewModel>>();

		return new List<RecentJobViewModel>();
	}



}
