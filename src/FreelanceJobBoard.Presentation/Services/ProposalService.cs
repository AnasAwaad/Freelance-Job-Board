using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class ProposalService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext? _httpContext;
	private readonly ILogger<CategoryService> _logger;

	public ProposalService(HttpClient httpClient,
		IHttpContextAccessor httpContextAccessor,
		ILogger<CategoryService> logger,
		IConfiguration configuration)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;

		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Proposals/");

		// Set authorization header if user is authenticated
		var token = _httpContext?.User?.FindFirst("jwt")?.Value;
		if (!string.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}
	}

	public async Task<bool> CreateProposalAsync(CreateProposalViewModel viewModel)
	{
		try
		{
			if (viewModel == null)
			{
				_logger.LogWarning("Attempted to create proposal with null viewModel");
				return false;
			}

			var response = await _httpClient.PostAsJsonAsync($"{viewModel.JobId}", viewModel);
			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Proposal created successfully for job {JobId}", viewModel.JobId);
				return true;
			}

			_logger.LogWarning("Failed to create proposal. Status: {StatusCode}", response.StatusCode);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating proposal");
			return false;
		}
		return false;
	}
}
