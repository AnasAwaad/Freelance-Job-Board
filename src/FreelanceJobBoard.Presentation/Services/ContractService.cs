using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetPendingChangeRequests;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;
using FreelanceJobBoard.Presentation.Models;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Text.Json;

namespace FreelanceJobBoard.Presentation.Services;

public class ContractService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContractService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContractService(HttpClient httpClient, ILogger<ContractService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        
        // Set base address to API root
        _httpClient.BaseAddress = new Uri("https://localhost:7000/api/");
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

    public async Task<GetUserContractsResult?> GetUserContractsAsync()
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("Contracts");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<GetUserContractsResult>(content, options);
            }

            _logger.LogWarning("Failed to retrieve user contracts. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user contracts");
            throw;
        }
    }

    public async Task<ContractDetailsDto?> GetContractDetailsAsync(int contractId)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Contracts/{contractId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<ContractDetailsDto>(content, options);
            }

            _logger.LogWarning("Failed to retrieve contract details for contract {ContractId}. Status: {StatusCode}", contractId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving contract details for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<GetContractHistoryResult?> GetContractHistoryAsync(int contractId)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Contracts/{contractId}/history");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<GetContractHistoryResult>(content, options);
            }

            _logger.LogWarning("Failed to retrieve contract history for contract {ContractId}. Status: {StatusCode}", contractId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving contract history for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<GetPendingChangeRequestsResult?> GetPendingChangeRequestsAsync()
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync("Contracts/pending-changes");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<GetPendingChangeRequestsResult>(content, options);
            }

            _logger.LogWarning("Failed to retrieve pending change requests. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving pending change requests");
            throw;
        }
    }

    public async Task<ServiceResult> ProposeContractChangesAsync(int contractId, ProposeContractChangeViewModel viewModel)
    {
        try
        {
            SetAuthorizationHeader();

            // Create multipart form data to support file uploads
            using var formData = new MultipartFormDataContent();
            
            // Add text fields
            formData.Add(new StringContent(viewModel.Title), "Title");
            formData.Add(new StringContent(viewModel.Description), "Description");
            formData.Add(new StringContent(viewModel.PaymentAmount.ToString()), "PaymentAmount");
            formData.Add(new StringContent(viewModel.PaymentType), "PaymentType");
            
            if (viewModel.ProjectDeadline.HasValue)
                formData.Add(new StringContent(viewModel.ProjectDeadline.Value.ToString("yyyy-MM-ddTHH:mm:ss")), "ProjectDeadline");
            
            if (!string.IsNullOrEmpty(viewModel.Deliverables))
                formData.Add(new StringContent(viewModel.Deliverables), "Deliverables");
                
            if (!string.IsNullOrEmpty(viewModel.TermsAndConditions))
                formData.Add(new StringContent(viewModel.TermsAndConditions), "TermsAndConditions");
                
            if (!string.IsNullOrEmpty(viewModel.AdditionalNotes))
                formData.Add(new StringContent(viewModel.AdditionalNotes), "AdditionalNotes");
                
            formData.Add(new StringContent(viewModel.ChangeReason), "ChangeReason");

            // Add files if any
            if (viewModel.AttachmentFiles?.Any() == true)
            {
                foreach (var file in viewModel.AttachmentFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        formData.Add(fileContent, "AttachmentFiles", file.FileName);
                    }
                }
            }

            var response = await _httpClient.PostAsync($"Contracts/{contractId}/propose-changes", formData);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            return await HandleErrorResponse(response, "proposing contract changes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while proposing contract changes for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<ServiceResult> RespondToChangeRequestAsync(int changeRequestId, bool isApproved, string? responseNotes = null)
    {
        try
        {
            SetAuthorizationHeader();

            var request = new
            {
                IsApproved = isApproved,
                ResponseNotes = responseNotes
            };

            var response = await _httpClient.PostAsJsonAsync($"Contracts/change-requests/{changeRequestId}/respond", request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            return await HandleErrorResponse(response, "responding to change request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while responding to change request {ChangeRequestId}", changeRequestId);
            throw;
        }
    }

    public async Task<ServiceResult> UpdateContractStatusAsync(int contractId, string status, string? notes = null)
    {
        try
        {
            SetAuthorizationHeader();

            var request = new
            {
                Status = status,
                Notes = notes
            };

            var response = await _httpClient.PutAsJsonAsync($"Contracts/{contractId}/status", request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            var statusCode = (int)response.StatusCode;
            var errorContent = await response.Content.ReadAsStringAsync();

            // Handle different error types
            if (statusCode == 400)
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (errorResponse.TryGetProperty("message", out var messageElement))
                    {
                        return ServiceResult.Failure(messageElement.GetString() ?? "Bad request", statusCode);
                    }
                }
                catch
                {
                    return ServiceResult.Failure(string.IsNullOrEmpty(errorContent) ? "Bad request" : errorContent, statusCode);
                }
            }

            if (statusCode == 401)
                return ServiceResult.Failure("You are not authorized to update this contract", statusCode);

            if (statusCode == 404)
                return ServiceResult.Failure("Contract not found", statusCode);

            return ServiceResult.Failure("An error occurred while updating contract status", statusCode);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error occurred while updating contract status for contract {ContractId}", contractId);
            return ServiceResult.Failure("Network error occurred. Please check your connection and try again.", 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating contract status for contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<ServiceResult> StartContractAsync(int contractId, string? notes = null)
    {
        try
        {
            SetAuthorizationHeader();

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/start", request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            return await HandleErrorResponse(response, "starting contract");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while starting contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<ServiceResult> CompleteContractAsync(int contractId, string? notes = null)
    {
        try
        {
            SetAuthorizationHeader();

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/complete", request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            return await HandleErrorResponse(response, "completing contract");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing contract {ContractId}", contractId);
            throw;
        }
    }

    public async Task<ServiceResult> CancelContractAsync(int contractId, string? notes = null)
    {
        try
        {
            SetAuthorizationHeader();

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/cancel", request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult.Success();
            }

            return await HandleErrorResponse(response, "cancelling contract");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while cancelling contract {ContractId}", contractId);
            throw;
        }
    }

    private async Task<ServiceResult> HandleErrorResponse(HttpResponseMessage response, string action)
    {
        var statusCode = (int)response.StatusCode;
        var errorContent = await response.Content.ReadAsStringAsync();

        if (statusCode == 400)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorResponse.TryGetProperty("message", out var messageElement))
                {
                    return ServiceResult.Failure(messageElement.GetString() ?? $"Bad request while {action}", statusCode);
                }
            }
            catch
            {
                return ServiceResult.Failure(string.IsNullOrEmpty(errorContent) ? $"Bad request while {action}" : errorContent, statusCode);
            }
        }

        if (statusCode == 401)
            return ServiceResult.Failure($"You are not authorized to perform this action", statusCode);

        if (statusCode == 404)
            return ServiceResult.Failure("Contract not found", statusCode);

        return ServiceResult.Failure($"An error occurred while {action}", statusCode);
    }
}