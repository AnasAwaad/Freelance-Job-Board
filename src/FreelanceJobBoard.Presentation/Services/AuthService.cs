using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;

namespace FreelanceJobBoard.Presentation.Services;

public class AuthService
{
	private readonly HttpClient _httpClient;

	public AuthService(HttpClient httpClient)
	{
		_httpClient = httpClient;
		_httpClient.BaseAddress = new Uri("http://localhost:5102/api/Auth/");
	}

	public async Task<AuthResponseDto> LoginAsync(LoginViewModel viewModel)
	{
		var user = new LoginDto
		{
			Email = viewModel.Email,
			Password = viewModel.Password,
		};

		var response = await _httpClient.PostAsJsonAsync("login", user);
		if (response.IsSuccessStatusCode)
		{
			return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
		}

		return null;
	}

	public async Task<bool> FreelancerRegister(FreelancerRegisterViewModel viewModel)
	{
		var user = new FreelancerRegisterDto
		{
			Email = viewModel.Email,
			FullName = viewModel.FullName,
			Password = viewModel.Password,
		};

		var response = await _httpClient.PostAsJsonAsync("freelancer-register", user);

		return response.IsSuccessStatusCode;
	}
}
