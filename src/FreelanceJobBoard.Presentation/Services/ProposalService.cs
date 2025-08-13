using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Presentation.Models;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Services;

public class ProposalService
{
	private readonly HttpClient _httpClient;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<ProposalService> _logger;

	public ProposalService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ProposalService> logger)
	{
		_httpClient = httpClient;
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
		_httpClient.BaseAddress = new Uri("https://localhost:7000/api/");
	}

	private string? GetCurrentUserId()
	{
		return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	}

	private void SetAuthorizationHeader()
	{
		var context = _httpContextAccessor.HttpContext;
		if (context?.User?.Identity?.IsAuthenticated == true)
		{
			var jwtToken = context.User.FindFirst("jwt")?.Value;
			if (!string.IsNullOrEmpty(jwtToken))
			{
				_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
			}
		}
	}

	private async Task<Dictionary<string, string[]>?> ParseValidationErrorsAsync(HttpResponseMessage response)
	{
		try
		{
			var errorContent = await response.Content.ReadAsStringAsync();
			var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			if (errorResponse.TryGetProperty("errors", out var errorsElement))
			{
				var validationErrors = new Dictionary<string, string[]>();

				foreach (var property in errorsElement.EnumerateObject())
				{
					var errorMessages = new List<string>();

					if (property.Value.ValueKind == JsonValueKind.Array)
					{
						foreach (var error in property.Value.EnumerateArray())
						{
							if (error.ValueKind == JsonValueKind.String)
							{
								errorMessages.Add(error.GetString() ?? "");
							}
						}
					}
					else if (property.Value.ValueKind == JsonValueKind.String)
					{
						errorMessages.Add(property.Value.GetString() ?? "");
					}

					validationErrors[property.Name] = errorMessages.ToArray();
				}

				return validationErrors;
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to parse validation errors from API response");
		}

		return null;
	}

	public async Task<List<ProposalViewModel>> GetFreelancerProposalsAsync()
	{
		try
		{
			SetAuthorizationHeader();
			var response = await _httpClient.GetAsync("Proposals/freelancer");

			if (response.IsSuccessStatusCode)
			{
				var proposalsJson = await response.Content.ReadAsStringAsync();
				var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(proposalsJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return proposals?.Select(p => new ProposalViewModel
				{
					Id = p.Id,
					JobId = p.JobId,
					JobTitle = p.JobTitle,
					JobDescription = p.JobDescription,
					JobBudgetMin = p.JobBudgetMin,
					JobBudgetMax = p.JobBudgetMax,
					JobDeadline = p.JobDeadline,
					CoverLetter = p.CoverLetter,
					BidAmount = p.BidAmount,
					EstimatedTimelineDays = p.EstimatedTimelineDays,
					Status = p.Status ?? "Submitted",
					ReviewedAt = p.ReviewedAt,
					ClientFeedback = p.ClientFeedback,
					ClientName = p.ClientName,
					ClientProfileImageUrl = p.ClientProfileImageUrl,
					ClientAverageRating = p.ClientAverageRating,
					ClientTotalReviews = p.ClientTotalReviews,
					Attachments = p.Attachments?.Select(a => new AttachmentViewModel
					{
						Id = a.Id,
						FileName = a.FileName,
						FileUrl = a.FileUrl,
						FileSize = a.FileSize
					}).ToList() ?? new List<AttachmentViewModel>()
				}).ToList() ?? new List<ProposalViewModel>();
			}

			_logger.LogWarning("Failed to retrieve freelancer proposals. Status: {StatusCode}", response.StatusCode);
			return new List<ProposalViewModel>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while retrieving freelancer proposals");
			throw;
		}
	}

	public async Task<List<ProposalViewModel>> GetJobProposalsAsync(int jobId, string? status = null)
	{
		try
		{
			SetAuthorizationHeader();
			var url = $"Proposals/job/{jobId}";
			if (!string.IsNullOrEmpty(status))
			{
				url += $"?status={status}";
			}

			var response = await _httpClient.GetAsync(url);

			if (response.IsSuccessStatusCode)
			{
				var proposalsJson = await response.Content.ReadAsStringAsync();
				var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(proposalsJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return proposals?.Select(p => new ProposalViewModel
				{
					Id = p.Id,
					JobId = p.JobId,
					JobTitle = p.JobTitle,
					JobDescription = p.JobDescription,
					JobBudgetMin = p.JobBudgetMin,
					JobBudgetMax = p.JobBudgetMax,
					JobDeadline = p.JobDeadline,
					CoverLetter = p.CoverLetter,
					BidAmount = p.BidAmount,
					EstimatedTimelineDays = p.EstimatedTimelineDays,
					Status = p.Status ?? "Submitted",
					ReviewedAt = p.ReviewedAt,
					ClientFeedback = p.ClientFeedback,
					FreelancerName = p.FreelancerName,
					FreelancerProfileImageUrl = p.FreelancerProfileImageUrl,
					FreelancerAverageRating = p.FreelancerAverageRating,
					FreelancerTotalReviews = p.FreelancerTotalReviews,
					Attachments = p.Attachments?.Select(a => new AttachmentViewModel
					{
						Id = a.Id,
						FileName = a.FileName,
						FileUrl = a.FileUrl,
						FileSize = a.FileSize
					}).ToList() ?? new List<AttachmentViewModel>()
				}).ToList() ?? new List<ProposalViewModel>();
			}

			_logger.LogWarning("Failed to retrieve job proposals for job {JobId}. Status: {StatusCode}", jobId, response.StatusCode);
			return new List<ProposalViewModel>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while retrieving proposals for job {JobId}", jobId);
			throw;
		}
	}

	public async Task<bool> SubmitProposalAsync(SubmitProposalViewModel viewModel)
	{
		try
		{
			SetAuthorizationHeader();

			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(viewModel.CoverLetter ?? ""), "CoverLetter");
			content.Add(new StringContent(viewModel.BidAmount.ToString()), "BidAmount");
			content.Add(new StringContent(viewModel.EstimatedTimelineDays.ToString()), "EstimatedTimelineDays");

			// Add portfolio files if any
			if (viewModel.PortfolioFiles != null)
			{
				foreach (var file in viewModel.PortfolioFiles)
				{
					if (file.Length > 0)
					{
						var fileContent = new StreamContent(file.OpenReadStream());
						fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
						content.Add(fileContent, "PortfolioFiles", file.FileName);
					}
				}
			}

			var response = await _httpClient.PostAsync($"Proposals/{viewModel.JobId}", content);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while submitting proposal for job {JobId}", viewModel.JobId);
			throw;
		}
	}

	public async Task<ServiceResult> UpdateProposalStatusAsync(int proposalId, string status, string? feedback = null)
	{
		try
		{
			SetAuthorizationHeader();

			var updateRequest = new
			{
				Status = status,
				ClientFeedback = feedback
			};

			var response = await _httpClient.PutAsJsonAsync($"Proposals/{proposalId}/status", updateRequest);

			if (response.IsSuccessStatusCode)
			{
				return ServiceResult.Success();
			}

			var statusCode = (int)response.StatusCode;

			// Handle validation errors (400 Bad Request)
			if (statusCode == 400)
			{
				var validationErrors = await ParseValidationErrorsAsync(response);
				if (validationErrors != null && validationErrors.Any())
				{
					return ServiceResult.ValidationFailure(validationErrors);
				}

				// If no validation errors but still 400, get the general error message
				var errorContent = await response.Content.ReadAsStringAsync();
				try
				{
					var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (errorResponse.TryGetProperty("title", out var titleElement))
					{
						return ServiceResult.Failure(titleElement.GetString() ?? "Bad request", statusCode);
					}
				}
				catch
				{
					// Fallback to raw content if JSON parsing fails
					return ServiceResult.Failure(string.IsNullOrEmpty(errorContent) ? "Bad request" : errorContent, statusCode);
				}
			}

			// Handle unauthorized (401)
			if (statusCode == 401)
			{
				return ServiceResult.Failure("You are not authorized to update this proposal", statusCode);
			}

			// Handle forbidden (403)
			if (statusCode == 403)
			{
				return ServiceResult.Failure("You do not have permission to update this proposal", statusCode);
			}

			// Handle not found (404)
			if (statusCode == 404)
			{
				return ServiceResult.Failure("Proposal not found", statusCode);
			}

			// Handle other client errors (4xx)
			if (statusCode >= 400 && statusCode < 500)
			{
				var errorContent = await response.Content.ReadAsStringAsync();
				return ServiceResult.Failure(string.IsNullOrEmpty(errorContent) ? "Client error occurred" : errorContent, statusCode);
			}

			// Handle server errors (5xx)
			return ServiceResult.Failure("Server error occurred while updating proposal status", statusCode);
		}
		catch (HttpRequestException httpEx)
		{
			_logger.LogError(httpEx, "HTTP error occurred while updating proposal {ProposalId} status", proposalId);
			return ServiceResult.Failure("Network error occurred. Please check your connection and try again.", 0);
		}
		catch (TaskCanceledException tcEx)
		{
			_logger.LogError(tcEx, "Request timeout while updating proposal {ProposalId} status", proposalId);
			return ServiceResult.Failure("Request timed out. Please try again.", 0);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error occurred while updating proposal {ProposalId} status", proposalId);
			return ServiceResult.Failure("An unexpected error occurred. Please try again.", 0);
		}
	}

	public async Task<ProposalViewModel?> GetProposalDetailsAsync(int proposalId)
	{
		try
		{
			SetAuthorizationHeader();
			var response = await _httpClient.GetAsync($"Proposals/{proposalId}");

			if (response.IsSuccessStatusCode)
			{
				var proposalJson = await response.Content.ReadAsStringAsync();
				var proposal = JsonSerializer.Deserialize<ProposalDto>(proposalJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (proposal != null)
				{
					return new ProposalViewModel
					{
						Id = proposal.Id,
						JobId = proposal.JobId,
						JobTitle = proposal.JobTitle,
						JobDescription = proposal.JobDescription,
						JobBudgetMin = proposal.JobBudgetMin,
						JobBudgetMax = proposal.JobBudgetMax,
						JobDeadline = proposal.JobDeadline,
						CoverLetter = proposal.CoverLetter,
						BidAmount = proposal.BidAmount,
						EstimatedTimelineDays = proposal.EstimatedTimelineDays,
						Status = proposal.Status ?? "Submitted",
						ReviewedAt = proposal.ReviewedAt,
						ClientFeedback = proposal.ClientFeedback,
						ClientName = proposal.ClientName,
						ClientProfileImageUrl = proposal.ClientProfileImageUrl,
						ClientAverageRating = proposal.ClientAverageRating,
						ClientTotalReviews = proposal.ClientTotalReviews,
						Attachments = proposal.Attachments?.Select(a => new AttachmentViewModel
						{
							Id = a.Id,
							FileName = a.FileName,
							FileUrl = a.FileUrl,
							FileSize = a.FileSize
						}).ToList() ?? new List<AttachmentViewModel>()
					};
				}
			}


		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error occurred while updating proposal {ProposalId} status", proposalId);
			return null;
		}
		return null;
	}

	public async Task<bool> DeleteProposalAsync(int proposalId)
	{
		try
		{
			SetAuthorizationHeader();
			var response = await _httpClient.DeleteAsync($"Proposals/{proposalId}");
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while deleting proposal {ProposalId}", proposalId);
			throw;
		}
	}

	public async Task<bool> HasFreelancerAppliedAsync(int jobId)
	{
		try
		{
			SetAuthorizationHeader();
			var response = await _httpClient.GetAsync($"Proposals/freelancer/applied/{jobId}");

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsStringAsync();
				return bool.TryParse(result, out var hasApplied) && hasApplied;
			}

			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while checking if freelancer has applied to job {JobId}", jobId);
			return false;
		}
	}

	public async Task<bool> HasJobAcceptedProposalAsync(int jobId)
	{
		try
		{
			SetAuthorizationHeader();
			var response = await _httpClient.GetAsync($"Proposals/job/{jobId}/has-accepted");

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadAsStringAsync();
				return bool.TryParse(result, out var hasAccepted) && hasAccepted;
			}

			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while checking if job {JobId} has accepted proposal", jobId);
			return false;
		}
	}

	public async Task<ServiceResult> RejectOtherProposalsAsync(int jobId, int acceptedProposalId)
	{
		try
		{
			SetAuthorizationHeader();

			var updateRequest = new
			{
				JobId = jobId,
				AcceptedProposalId = acceptedProposalId
			};

			var response = await _httpClient.PostAsJsonAsync($"Proposals/reject-others", updateRequest);

			if (response.IsSuccessStatusCode)
			{
				return ServiceResult.Success();
			}

			var statusCode = (int)response.StatusCode;
			var errorContent = await response.Content.ReadAsStringAsync();
			return ServiceResult.Failure(string.IsNullOrEmpty(errorContent) ? "Failed to reject other proposals" : errorContent, statusCode);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error occurred while rejecting other proposals for job {JobId}", jobId);
			return ServiceResult.Failure("An unexpected error occurred. Please try again.", 0);
		}
	}
}
