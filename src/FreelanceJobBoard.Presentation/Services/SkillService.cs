using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class SkillService
{
	private readonly HttpClient _httpClient;
	private readonly HttpContext? _httpContext;
	private readonly ILogger<SkillService> _logger;

	public SkillService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<SkillService> logger)
	{
		_httpClient = httpClient;
		_httpContext = httpContextAccessor.HttpContext;
		_logger = logger;

		// Fix URL format
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Skills/");

		// Set authorization header if user is authenticated
		var token = _httpContext?.User?.FindFirst("jwt")?.Value;
		if (!string.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		}
	}

	public async Task<IEnumerable<SkillViewModel>> GetAllSkillsAsync(string? search = null, bool? isActive = null)
	{
		try
		{
			var queryParams = new List<string>();

			if (!string.IsNullOrEmpty(search))
				queryParams.Add($"search={Uri.EscapeDataString(search)}");

			if (isActive.HasValue)
				queryParams.Add($"isActive={isActive.Value}");

			var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
			var response = await _httpClient.GetAsync($"{queryString}");

			if (response.IsSuccessStatusCode)
			{
				var skills = await response.Content.ReadFromJsonAsync<IEnumerable<SkillViewModel>>();
				return skills ?? new List<SkillViewModel>();
			}

			_logger.LogWarning("Failed to get skills. Status: {StatusCode}", response.StatusCode);
			return new List<SkillViewModel>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching skills");
			return new List<SkillViewModel>();
		}
	}

	public async Task<SkillViewModel?> GetSkillByIdAsync(int id)
	{
		try
		{
			if (id <= 0)
			{
				_logger.LogWarning("Invalid skill ID: {Id}", id);
				return null;
			}

			var response = await _httpClient.GetAsync($"{id}");

			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<SkillViewModel>();
			}

			_logger.LogWarning("Failed to get skill by ID {Id}. Status: {StatusCode}", id, response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching skill by ID {Id}", id);
			return null;
		}
	}

	public async Task<int?> CreateSkillAsync(CreateSkillViewModel viewModel)
	{
		try
		{
			if (viewModel == null)
			{
				_logger.LogWarning("Attempted to create skill with null viewModel");
				return null;
			}

			var response = await _httpClient.PostAsJsonAsync("", viewModel);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				if (int.TryParse(content, out int skillId))
				{
					_logger.LogInformation("Skill created successfully with ID {SkillId}", skillId);
					return skillId;
				}
			}

			_logger.LogWarning("Failed to create skill. Status: {StatusCode}", response.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating skill");
			return null;
		}
	}

	public async Task<bool> UpdateSkillAsync(UpdateSkillViewModel viewModel)
	{
		try
		{
			if (viewModel == null)
			{
				_logger.LogWarning("Attempted to update skill with null viewModel");
				return false;
			}

			var response = await _httpClient.PutAsJsonAsync($"{viewModel.Id}", viewModel);

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Skill updated successfully with ID {SkillId}", viewModel.Id);
				return true;
			}

			_logger.LogWarning("Failed to update skill {Id}. Status: {StatusCode}", viewModel.Id, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while updating skill {Id}", viewModel?.Id);
			return false;
		}
	}

	public async Task<bool> DeleteSkillAsync(int id)
	{
		try
		{
			if (id <= 0)
			{
				_logger.LogWarning("Invalid skill ID for deletion: {Id}", id);
				return false;
			}

			var response = await _httpClient.DeleteAsync($"{id}");

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Skill deleted successfully with ID {SkillId}", id);
				return true;
			}

			_logger.LogWarning("Failed to delete skill {Id}. Status: {StatusCode}", id, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting skill {Id}", id);
			return false;
		}
	}

	public async Task<bool> ToggleSkillStatusAsync(int id)
	{
		try
		{
			if (id <= 0)
			{
				_logger.LogWarning("Invalid skill ID for status toggle: {Id}", id);
				return false;
			}

			var response = await _httpClient.PostAsync($"{id}/toggle-status", null);

			if (response.IsSuccessStatusCode)
			{
				_logger.LogInformation("Skill status toggled successfully for ID {SkillId}", id);
				return true;
			}

			_logger.LogWarning("Failed to toggle skill status for ID {Id}. Status: {StatusCode}", id, response.StatusCode);
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while toggling skill status for ID {Id}", id);
			return false;
		}
	}
}