using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class NotificationService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext? _httpContext;
	private readonly ILogger<NotificationService> _logger;

	public NotificationService(HttpClient httpClient,
		IHttpContextAccessor httpContextAccessor,
		ILogger<NotificationService> logger,
		IConfiguration configuration)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;

		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Notifications/");

		var token = _httpContext?.User?.FindFirst("jwt")?.Value;
		if (!string.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}
	}

	public async Task<IEnumerable<NotificationViewModel>> GetAllNotificationsAsync()
	{
		try
		{
			var response = await _httpClient.GetAsync("user");

			if (response.IsSuccessStatusCode)
			{
				var notifications = await response.Content.ReadFromJsonAsync<IEnumerable<NotificationViewModel>>();
				return notifications ?? new List<NotificationViewModel>();
			}

			_logger.LogWarning("Failed to get categories. Status: {StatusCode}", response.StatusCode);
			return new List<NotificationViewModel>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching categories");
			return new List<NotificationViewModel>();
		}
	}
}
