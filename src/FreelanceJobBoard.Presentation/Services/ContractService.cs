using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractDetails;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetContractHistory;
using FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;
using FreelanceJobBoard.Presentation.Models;
using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using System.Diagnostics;

namespace FreelanceJobBoard.Presentation.Services;

public class ContractService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ContractService> _logger;

    public ContractService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ContractService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://localhost:7000/api/");
    }

    public async Task<GetUserContractsResult?> GetAllContractsAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        var sessionId = _httpContextAccessor.HttpContext?.Session?.Id ?? "NoSession";
        
        try
        {
            _logger.LogInformation("?? Fetching all contracts | User={UserId}, Session={SessionId}", userId, sessionId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("GET_ALL_CONTRACTS");

            var response = await _httpClient.GetAsync("Contracts");

            _logger.LogDebug("?? API Response | Status={StatusCode} {ReasonPhrase}, ContentType='{ContentType}'", 
                (int)response.StatusCode, response.ReasonPhrase, 
                response.Content.Headers.ContentType?.ToString() ?? "unknown");

            if (response.IsSuccessStatusCode)
            {
                var contracts = await response.Content.ReadFromJsonAsync<GetUserContractsResult>();
                
                stopwatch.Stop();
                var contractCount = contracts?.Contracts?.Count() ?? 0;
                _logger.LogInformation("? Contracts fetched successfully! Count={ContractCount}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractCount, userId, stopwatch.ElapsedMilliseconds);

                // Log contract status breakdown
                if (contracts?.Contracts?.Any() == true)
                {
                    var statusBreakdown = contracts.Contracts.GroupBy(c => c.ContractStatus).ToDictionary(g => g.Key, g => g.Count());
                    _logger.LogDebug("?? Contract Status Breakdown | User={UserId}, StatusBreakdown={@StatusBreakdown}", 
                        userId, statusBreakdown);
                }

                // Log performance warning
                if (stopwatch.ElapsedMilliseconds > 2000)
                {
                    _logger.LogWarning("?? Slow contract fetch | Duration={ElapsedMs}ms, User={UserId}", 
                        stopwatch.ElapsedMilliseconds, userId);
                }

                return contracts;
            }

            stopwatch.Stop();
            _logger.LogWarning("?? Failed to get contracts | User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
                userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while fetching contracts | User={UserId}, Duration={ElapsedMs}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "? Request timeout while fetching contracts | User={UserId}, Duration={ElapsedMs}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while fetching contracts | User={UserId}, Duration={ElapsedMs}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    public async Task<ContractDetailsDto?> GetContractByIdAsync(int contractId)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        var sessionId = _httpContextAccessor.HttpContext?.Session?.Id ?? "NoSession";
        
        try
        {
            _logger.LogInformation("?? Fetching contract details | ContractId={ContractId}, User={UserId}, Session={SessionId}", 
                contractId, userId, sessionId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("GET_CONTRACT_BY_ID");

            var response = await _httpClient.GetAsync($"Contracts/{contractId}");

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var contract = await response.Content.ReadFromJsonAsync<ContractDetailsDto>();
                
                stopwatch.Stop();
                _logger.LogInformation("? Contract details fetched successfully! ContractId={ContractId}, Status='{Status}', User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, contract?.ContractStatus ?? "Unknown", userId, stopwatch.ElapsedMilliseconds);

                // Log contract insights
                if (contract != null)
                {
                    _logger.LogDebug("?? Contract Info | ContractId={ContractId}, PaymentAmount=${PaymentAmount}, Client='{ClientName}', Freelancer='{FreelancerName}'", 
                        contractId, contract.PaymentAmount, contract.ClientName ?? "Unknown", contract.FreelancerName ?? "Unknown");
                }

                return contract;
            }

            stopwatch.Stop();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("? Contract not found | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("?? Failed to get contract details | ContractId={ContractId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
                    contractId, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while fetching contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while fetching contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    public async Task<GetContractHistoryResult?> GetContractHistoryAsync(int contractId)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        var sessionId = _httpContextAccessor.HttpContext?.Session?.Id ?? "NoSession";
        
        try
        {
            _logger.LogInformation("?? Fetching contract history | ContractId={ContractId}, User={UserId}, Session={SessionId}", 
                contractId, userId, sessionId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("GET_CONTRACT_HISTORY");

            var response = await _httpClient.GetAsync($"Contracts/{contractId}/history");

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var history = await response.Content.ReadFromJsonAsync<GetContractHistoryResult>();
                
                stopwatch.Stop();
                var versionCount = history?.VersionHistory?.Count() ?? 0;
                var changeRequestCount = history?.ChangeRequests?.Count() ?? 0;
                
                _logger.LogInformation("? Contract history fetched successfully! ContractId={ContractId}, Versions={VersionCount}, ChangeRequests={ChangeRequestCount}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, versionCount, changeRequestCount, userId, stopwatch.ElapsedMilliseconds);

                // Log version details
                if (history?.VersionHistory?.Any() == true)
                {
                    var currentVersion = history.CurrentVersion;
                    _logger.LogDebug("?? Version Info | ContractId={ContractId}, CurrentVersion={CurrentVersionNumber}, TotalVersions={TotalVersions}", 
                        contractId, currentVersion?.VersionNumber ?? 0, versionCount);
                }

                return history;
            }

            stopwatch.Stop();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("? Contract history not found | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning("?? Failed to get contract history | ContractId={ContractId}, User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
                    contractId, userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
            }
            return null;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while fetching contract history | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while fetching contract history | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    public async Task<ServiceResult> UpdateContractStatusAsync(int contractId, string status, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Updating contract status | ContractId={ContractId}, NewStatus='{Status}', User={UserId}", 
                contractId, status, userId);
            _logger.LogDebug("?? Status Update Details | ContractId={ContractId}, Status='{Status}', Notes='{Notes}'", 
                contractId, status, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("UPDATE_CONTRACT_STATUS");

            var request = new { Status = status, Notes = notes };
            var response = await _httpClient.PutAsJsonAsync($"Contracts/{contractId}/status", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Contract status updated successfully! ContractId={ContractId}, NewStatus='{Status}', User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, status, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "updating contract status");
            _logger.LogWarning("?? Failed to update contract status | ContractId={ContractId}, Status='{Status}', User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, status, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while updating contract status | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while updating contract status | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ServiceResult> StartContractAsync(int contractId, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Starting contract | ContractId={ContractId}, User={UserId}", contractId, userId);
            _logger.LogDebug("?? Start Details | ContractId={ContractId}, Notes='{Notes}'", contractId, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("START_CONTRACT");

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/start", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Contract started successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "starting contract");
            _logger.LogWarning("?? Failed to start contract | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while starting contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while starting contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ServiceResult> CompleteContractAsync(int contractId, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Requesting contract completion | ContractId={ContractId}, User={UserId}", contractId, userId);
            _logger.LogDebug("?? Completion Details | ContractId={ContractId}, Notes='{Notes}'", contractId, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("REQUEST_CONTRACT_COMPLETION");

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/complete", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Contract completion requested successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "requesting contract completion");
            _logger.LogWarning("?? Failed to request contract completion | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while requesting contract completion | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while requesting contract completion | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ServiceResult> ApproveCompletionAsync(int contractId, bool isApproved, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("? {Action} contract completion | ContractId={ContractId}, User={UserId}", 
                isApproved ? "Approving" : "Rejecting", contractId, userId);
            _logger.LogDebug("?? Approval Details | ContractId={ContractId}, IsApproved={IsApproved}, Notes='{Notes}'", 
                contractId, isApproved, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("APPROVE_CONTRACT_COMPLETION");

            var request = new { IsApproved = isApproved, Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/approve-completion", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                var action = isApproved ? "approved" : "rejected";
                _logger.LogInformation("? Contract completion {Action} successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    action, contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, $"{(isApproved ? "approving" : "rejecting")} contract completion");
            _logger.LogWarning("?? Failed to {Action} contract completion | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                isApproved ? "approve" : "reject", contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while {Action} contract completion | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                isApproved ? "approving" : "rejecting", contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while {Action} contract completion | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                isApproved ? "approving" : "rejecting", contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ServiceResult> CancelContractAsync(int contractId, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Cancelling contract | ContractId={ContractId}, User={UserId}", contractId, userId);
            _logger.LogDebug("?? Cancel Details | ContractId={ContractId}, Notes='{Notes}'", contractId, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("CANCEL_CONTRACT");

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/cancel", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Contract cancelled successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "cancelling contract");
            _logger.LogWarning("?? Failed to cancel contract | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while cancelling contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while cancelling contract | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<ServiceResult> CancelCompletionRequestAsync(int contractId, string? notes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Cancelling completion request | ContractId={ContractId}, User={UserId}", contractId, userId);
            _logger.LogDebug("?? Cancel Request Details | ContractId={ContractId}, Notes='{Notes}'", contractId, notes ?? "None");
            
            SetAuthorizationHeader();
            LogRequestHeaders("CANCEL_COMPLETION_REQUEST");

            var request = new { Notes = notes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/cancel-completion-request", request);

            _logger.LogDebug("?? API Response | ContractId={ContractId}, Status={StatusCode} {ReasonPhrase}", 
                contractId, (int)response.StatusCode, response.StatusCode, response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Completion request cancelled successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "cancelling completion request");
            _logger.LogWarning("?? Failed to cancel completion request | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while cancelling completion request | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while cancelling completion request | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    // Compatibility aliases for existing controller methods
    public async Task<GetUserContractsResult?> GetUserContractsAsync()
    {
        return await GetAllContractsAsync();
    }

    public async Task<ContractDetailsDto?> GetContractDetailsAsync(int contractId)
    {
        return await GetContractByIdAsync(contractId);
    }

    // TODO: These methods need to be implemented based on the actual API endpoints
    public async Task<ServiceResult> ProposeContractChangesAsync(int contractId, ProposeContractChangeViewModel model)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Proposing contract changes | ContractId={ContractId}, User={UserId}", contractId, userId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("PROPOSE_CONTRACT_CHANGES");

            var response = await _httpClient.PostAsJsonAsync($"Contracts/{contractId}/changes", model);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Contract changes proposed successfully! ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                    contractId, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "proposing contract changes");
            _logger.LogWarning("?? Failed to propose contract changes | ContractId={ContractId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                contractId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while proposing contract changes | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while proposing contract changes | ContractId={ContractId}, User={UserId}, Duration={ElapsedMs}ms", 
                contractId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<object?> GetPendingChangeRequestsAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("? Fetching pending change requests | User={UserId}", userId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("GET_PENDING_CHANGE_REQUESTS");

            var response = await _httpClient.GetAsync("Contracts/pending-changes");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<object>();
                stopwatch.Stop();
                _logger.LogInformation("? Pending change requests fetched successfully! User={UserId}, Duration={ElapsedMs}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
                return result;
            }

            stopwatch.Stop();
            _logger.LogWarning("?? Failed to get pending change requests | User={UserId}, Status={StatusCode} {ReasonPhrase}, Duration={ElapsedMs}ms", 
                userId, (int)response.StatusCode, response.ReasonPhrase, stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while fetching pending change requests | User={UserId}, Duration={ElapsedMs}ms", 
                userId, stopwatch.ElapsedMilliseconds);
            return null;
        }
    }

    public async Task<ServiceResult> RespondToChangeRequestAsync(int changeRequestId, bool isApproved, string? responseNotes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
        
        try
        {
            _logger.LogInformation("?? Responding to change request | ChangeRequestId={ChangeRequestId}, IsApproved={IsApproved}, User={UserId}", 
                changeRequestId, isApproved, userId);
            
            SetAuthorizationHeader();
            LogRequestHeaders("RESPOND_TO_CHANGE_REQUEST");

            var request = new { IsApproved = isApproved, ResponseNotes = responseNotes };
            var response = await _httpClient.PostAsJsonAsync($"Contracts/change-requests/{changeRequestId}/respond", request);

            if (response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                _logger.LogInformation("? Change request response submitted successfully! ChangeRequestId={ChangeRequestId}, IsApproved={IsApproved}, User={UserId}, Duration={ElapsedMs}ms", 
                    changeRequestId, isApproved, userId, stopwatch.ElapsedMilliseconds);
                return ServiceResult.Success();
            }

            stopwatch.Stop();
            var errorResult = await HandleErrorResponse(response, "responding to change request");
            _logger.LogWarning("?? Failed to respond to change request | ChangeRequestId={ChangeRequestId}, User={UserId}, ApiStatus={StatusCode}, Error={ErrorMessage}, Duration={ElapsedMs}ms", 
                changeRequestId, userId, response.StatusCode, errorResult.ErrorMessage, stopwatch.ElapsedMilliseconds);
            
            return errorResult;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? HTTP error while responding to change request | ChangeRequestId={ChangeRequestId}, User={UserId}, Duration={ElapsedMs}ms", 
                changeRequestId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? Unexpected error while responding to change request | ChangeRequestId={ChangeRequestId}, User={UserId}, Duration={ElapsedMs}ms", 
                changeRequestId, userId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private void SetAuthorizationHeader()
    {
        try
        {
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                _logger.LogDebug("?? Authorization header set | HasToken={HasToken}", !string.IsNullOrEmpty(token));
            }
            else
            {
                _logger.LogDebug("?? No JWT token found for authorization");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set authorization header");
        }
    }

    private void LogRequestHeaders(string operation)
    {
        try
        {
            var headers = new Dictionary<string, string>();

            // Log important headers
            foreach (var header in _httpClient.DefaultRequestHeaders)
            {
                if (IsImportantHeader(header.Key))
                {
                    headers[header.Key] = GetSafeHeaderValue(string.Join(", ", header.Value));
                }
            }

            if (headers.Any())
            {
                _logger.LogDebug("?? {Operation} - Request Headers: {@Headers}", operation, headers);
            }

            // Log authorization status
            var hasAuth = _httpClient.DefaultRequestHeaders.Authorization != null;
            _logger.LogDebug("?? Authorization Status | Operation={Operation}, HasAuth={HasAuth}, Scheme={Scheme}", 
                operation, hasAuth, _httpClient.DefaultRequestHeaders.Authorization?.Scheme ?? "none");

            // Log user context
            var userClaims = _httpContextAccessor.HttpContext?.User?.Claims?.Where(c => !c.Type.Contains("nbf") && !c.Type.Contains("exp") && !c.Type.Contains("iat"))
                .ToDictionary(c => c.Type.Split('/').Last(), c => c.Value);
            
            if (userClaims?.Any() == true)
            {
                _logger.LogDebug("?? User Context | Operation={Operation}, Claims={@Claims}", operation, userClaims);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to log request headers for operation {Operation}", operation);
        }
    }

    private static bool IsImportantHeader(string headerName)
    {
        var important = new[] 
        {
            "Authorization", "Accept", "Content-Type", "User-Agent"
        };
        return important.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetSafeHeaderValue(string headerValue)
    {
        if (string.IsNullOrEmpty(headerValue))
            return "";

        // Redact sensitive headers
        if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return "Bearer [REDACTED]";

        return headerValue.Length > 100 ? headerValue[..100] + "[TRUNCATED]" : headerValue;
    }

    private async Task<ServiceResult> HandleErrorResponse(HttpResponseMessage response, string operation)
    {
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;
            
            _logger.LogDebug("?? Error Response Content | Operation={Operation}, StatusCode={StatusCode}, Content='{Content}'", 
                operation, statusCode, errorContent.Length > 200 ? errorContent[..200] + "..." : errorContent);

            return statusCode switch
            {
                400 => ServiceResult.Failure("Invalid request data provided"),
                401 => ServiceResult.Failure("You are not authorized to perform this action"),
                403 => ServiceResult.Failure("You do not have permission to perform this action"),
                404 => ServiceResult.Failure("The requested resource was not found"),
                409 => ServiceResult.Failure("This action conflicts with the current state"),
                _ => ServiceResult.Failure($"An error occurred: {response.ReasonPhrase}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse error response for operation {Operation}", operation);
            return ServiceResult.Failure("An unexpected error occurred");
        }
    }
}