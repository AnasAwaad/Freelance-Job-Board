using FreelanceJobBoard.Application.Features.Dashboard.DTOs;

namespace FreelanceJobBoard.Application.Interfaces.Services;

public interface IDashboardService
{
    // Core dashboard statistics
    Task<DashboardStatsDto> GetDashboardStatsAsync(string userId);
    Task<RecentActivityDto> GetRecentActivityAsync(string userId, int limit = 10);
    
    // Role-specific analytics
    Task<ClientDashboardDto> GetClientDashboardAsync(string clientUserId);
    Task<FreelancerDashboardDto> GetFreelancerDashboardAsync(string freelancerUserId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    
    // Additional analytics
    Task<EarningsAnalyticsDto> GetEarningsAnalyticsAsync(string freelancerUserId);
    Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(string clientUserId);
    Task<JobApplicationFunnelDto> GetJobApplicationFunnelAsync(string userId);
    Task<ProposalSuccessRateDto> GetProposalSuccessRateAsync(string freelancerUserId);
    Task<ContractCompletionRateDto> GetContractCompletionRateAsync(string userId);
    
    // Admin-specific analytics
    Task<TopUsersDto> GetTopFreelancersAsync(int limit = 10);
    Task<TopUsersDto> GetTopClientsAsync(int limit = 10);
    Task<decimal> GetAverageJobApprovalTimeAsync();
}