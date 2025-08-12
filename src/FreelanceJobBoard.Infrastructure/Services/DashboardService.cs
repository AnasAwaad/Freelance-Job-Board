using FreelanceJobBoard.Application.Features.Dashboard.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace FreelanceJobBoard.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId)
    {
        try
        {
            var stats = new DashboardStatsDto
            {
                UserRole = "User",
                ActiveJobs = 0,
                Proposals = 0,
                Contracts = 0,
                Reviews = 0,
                PendingApprovals = 0
            };

            // Get basic counts - simplified approach for now
            var allJobs = await _unitOfWork.Jobs.GetAllAsync();
            var allProposals = await _unitOfWork.Proposals.GetAllAsync();
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();

            stats.ActiveJobs = allJobs.Count(j => j.Status == "Active" || j.Status == "InProgress");
            stats.Proposals = allProposals.Count();
            stats.Contracts = allContracts.Count();
            stats.Reviews = allReviews.Count();
            stats.PendingApprovals = allJobs.Count(j => j.Status == "PendingApproval");

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboard stats for user {UserId}", userId);
            return new DashboardStatsDto { UserRole = "User" };
        }
    }

    public async Task<RecentActivityDto> GetRecentActivityAsync(string userId, int limit = 10)
    {
        try
        {
            var activities = new List<ActivityItemDto>();

            // Get recent notifications as activities
            var notifications = await _unitOfWork.Notifications.GetRecentNotificationsAsync(userId, limit);
            foreach (var notification in notifications.Take(limit))
            {
                activities.Add(new ActivityItemDto
                {
                    Id = notification.Id.ToString(),
                    Title = notification.Title,
                    Description = notification.Message,
                    Timestamp = notification.CreatedOn,
                    Type = notification.Type ?? "notification",
                    Icon = GetIconForNotificationType(notification.Type ?? ""),
                    Color = GetColorForNotificationType(notification.Type ?? ""),
                    ActionUrl = GetActionUrlForNotification(notification)
                });
            }

            return new RecentActivityDto { Activities = activities.OrderByDescending(a => a.Timestamp).ToList() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recent activity for user {UserId}", userId);
            return new RecentActivityDto();
        }
    }

    public async Task<ClientDashboardDto> GetClientDashboardAsync(string clientUserId)
    {
        try
        {
            var dashboard = new ClientDashboardDto
            {
                UserRole = AppRoles.Client,
                JobsPosted = 0,
                JobsPendingApproval = 0,
                ActiveJobs = 0,
                ProposalsReceived = 0,
                Proposals = 0,
                ActiveContracts = 0,
                CompletedContracts = 0,
                Contracts = 0,
                Reviews = 0,
                PendingApprovals = 0,
                TotalSpent = 0,
                MonthlySpent = 0
            };

            // Simplified implementation - you can enhance this later
            var allJobs = await _unitOfWork.Jobs.GetAllAsync();
            var allProposals = await _unitOfWork.Proposals.GetAllAsync();
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();

            dashboard.JobsPosted = allJobs.Count();
            dashboard.ActiveJobs = allJobs.Count(j => j.Status == "Active");
            dashboard.JobsPendingApproval = allJobs.Count(j => j.Status == "PendingApproval");
            dashboard.ProposalsReceived = allProposals.Count();
            dashboard.Proposals = allProposals.Count();
            dashboard.ActiveContracts = allContracts.Count();
            dashboard.CompletedContracts = allContracts.Count();
            dashboard.Contracts = allContracts.Count();
            dashboard.Reviews = allReviews.Count();
            dashboard.PendingApprovals = dashboard.JobsPendingApproval;

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get client dashboard for user {UserId}", clientUserId);
            return new ClientDashboardDto { UserRole = AppRoles.Client };
        }
    }

    public async Task<FreelancerDashboardDto> GetFreelancerDashboardAsync(string freelancerUserId)
    {
        try
        {
            var dashboard = new FreelancerDashboardDto
            {
                UserRole = AppRoles.Freelancer,
                ProposalsSubmitted = 0,
                ProposalsAccepted = 0,
                Proposals = 0,
                ActiveContracts = 0,
                CompletedContracts = 0,
                Contracts = 0,
                Reviews = 0,
                ActiveJobs = 0,
                TotalEarnings = 0,
                MonthlyEarnings = 0,
                SuccessRate = 0,
                AverageRating = 0
            };

            // Simplified implementation - you can enhance this later
            var allProposals = await _unitOfWork.Proposals.GetAllAsync();
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();

            dashboard.ProposalsSubmitted = allProposals.Count();
            dashboard.ProposalsAccepted = allProposals.Count(p => p.Status == "Accepted");
            dashboard.Proposals = allProposals.Count();
            dashboard.ActiveContracts = allContracts.Count();
            dashboard.CompletedContracts = allContracts.Count();
            dashboard.Contracts = allContracts.Count();
            dashboard.Reviews = allReviews.Count();
            dashboard.ActiveJobs = allContracts.Count();

            if (dashboard.ProposalsSubmitted > 0)
            {
                dashboard.SuccessRate = (double)dashboard.ProposalsAccepted / dashboard.ProposalsSubmitted * 100;
            }

            if (allReviews.Any())
            {
                dashboard.AverageRating = allReviews.Average(r => r.Rating);
            }

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get freelancer dashboard for user {UserId}", freelancerUserId);
            return new FreelancerDashboardDto { UserRole = AppRoles.Freelancer };
        }
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        try
        {
            var allJobs = await _unitOfWork.Jobs.GetAllAsync();
            var allProposals = await _unitOfWork.Proposals.GetAllAsync();
            var allContracts = await _unitOfWork.Contracts.GetAllAsync();
            var allReviews = await _unitOfWork.Reviews.GetAllAsync();

            var dashboard = new AdminDashboardDto
            {
                UserRole = AppRoles.Admin,
                TotalUsers = 100, // Placeholder
                TotalFreelancers = 50, // Placeholder
                TotalClients = 50, // Placeholder
                JobsPendingApproval = allJobs.Count(j => j.Status == "PendingApproval"),
                FlaggedJobs = allJobs.Count(j => j.Status == "Rejected"),
                TotalJobsPosted = allJobs.Count(),
                TotalContractsCompleted = allContracts.Count(),
                ActiveJobs = allJobs.Count(j => j.Status == "Active"),
                Proposals = allProposals.Count(),
                Contracts = allContracts.Count(),
                Reviews = allReviews.Count(),
                PendingApprovals = allJobs.Count(j => j.Status == "PendingApproval"),
                PlatformRevenue = 10000m, // Placeholder
                AverageJobApprovalTime = 24.0 // Placeholder: 24 hours
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get admin dashboard");
            return new AdminDashboardDto { UserRole = AppRoles.Admin };
        }
    }

    public async Task<EarningsAnalyticsDto> GetEarningsAnalyticsAsync(string freelancerUserId)
    {
        try
        {
            var analytics = new EarningsAnalyticsDto
            {
                TotalEarnings = 0,
                CurrentMonthEarnings = 0,
                LastMonthEarnings = 0,
                MonthlyTrend = new List<MonthlyEarningsDto>(),
                EarningsByCategory = new List<CategoryEarningsDto>()
            };

            // Simplified implementation
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get earnings analytics for freelancer {UserId}", freelancerUserId);
            return new EarningsAnalyticsDto();
        }
    }

    public async Task<SpendingAnalyticsDto> GetSpendingAnalyticsAsync(string clientUserId)
    {
        try
        {
            var analytics = new SpendingAnalyticsDto
            {
                TotalSpent = 0,
                CurrentMonthSpent = 0,
                LastMonthSpent = 0,
                MonthlyTrend = new List<MonthlySpendingDto>(),
                SpendingByCategory = new List<CategorySpendingDto>()
            };

            // Simplified implementation
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get spending analytics for client {UserId}", clientUserId);
            return new SpendingAnalyticsDto();
        }
    }

    public async Task<JobApplicationFunnelDto> GetJobApplicationFunnelAsync(string userId)
    {
        try
        {
            var funnel = new JobApplicationFunnelDto
            {
                JobsPosted = 0,
                ProposalsReceived = 0,
                ContractsSigned = 0,
                ProjectsCompleted = 0,
                ProposalConversionRate = 0,
                ContractConversionRate = 0,
                CompletionRate = 0
            };

            // Simplified implementation
            return funnel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job application funnel for user {UserId}", userId);
            return new JobApplicationFunnelDto();
        }
    }

    public async Task<ProposalSuccessRateDto> GetProposalSuccessRateAsync(string freelancerUserId)
    {
        try
        {
            var successRate = new ProposalSuccessRateDto
            {
                ProposalsSubmitted = 0,
                ProposalsAccepted = 0,
                SuccessRate = 0,
                MonthlyTrend = new List<MonthlySuccessRateDto>()
            };

            // Simplified implementation
            return successRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get proposal success rate for freelancer {UserId}", freelancerUserId);
            return new ProposalSuccessRateDto();
        }
    }

    public async Task<ContractCompletionRateDto> GetContractCompletionRateAsync(string userId)
    {
        try
        {
            var completionRate = new ContractCompletionRateDto
            {
                TotalContracts = 0,
                CompletedContracts = 0,
                CompletionRate = 0,
                MonthlyTrend = new List<MonthlyCompletionRateDto>()
            };

            // Simplified implementation
            return completionRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get contract completion rate for user {UserId}", userId);
            return new ContractCompletionRateDto();
        }
    }

    public async Task<TopUsersDto> GetTopFreelancersAsync(int limit = 10)
    {
        try
        {
            var topUsers = new TopUsersDto
            {
                Users = new List<TopUserDto>()
            };

            // Simplified implementation
            return topUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top freelancers");
            return new TopUsersDto();
        }
    }

    public async Task<TopUsersDto> GetTopClientsAsync(int limit = 10)
    {
        try
        {
            var topUsers = new TopUsersDto
            {
                Users = new List<TopUserDto>()
            };

            // Simplified implementation
            return topUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top clients");
            return new TopUsersDto();
        }
    }

    public async Task<decimal> GetAverageJobApprovalTimeAsync()
    {
        try
        {
            // Simplified implementation
            return 24.0m; // Return 24 hours as placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get average job approval time");
            return 0;
        }
    }

    private static string GetIconForNotificationType(string type)
    {
        return type switch
        {
            "proposal_received" or "proposal_submitted" => "ki-document",
            "proposal_accepted" => "ki-check-circle",
            "proposal_rejected" => "ki-cross-circle",
            "job_posted" or "job_created" => "ki-briefcase",
            "job_approved" => "ki-check",
            "job_rejected" => "ki-cross",
            "contract_created" => "ki-handshake",
            "contract_completed" => "ki-check-circle",
            "review_received" => "ki-star",
            "payment_received" => "ki-dollar",
            _ => "ki-notification-bing"
        };
    }

    private static string GetColorForNotificationType(string type)
    {
        return type switch
        {
            "proposal_accepted" or "job_approved" or "contract_completed" => "success",
            "proposal_rejected" or "job_rejected" => "danger",
            "proposal_received" or "job_posted" => "primary",
            "contract_created" => "info",
            "review_received" => "warning",
            "payment_received" => "success",
            _ => "secondary"
        };
    }

    private static string? GetActionUrlForNotification(Domain.Entities.Notification notification)
    {
        return notification.Type switch
        {
            "proposal_received" or "proposal_submitted" when notification.JobId.HasValue => $"/Jobs/Details/{notification.JobId}",
            "job_posted" or "job_approved" or "job_rejected" when notification.JobId.HasValue => $"/Jobs/Details/{notification.JobId}",
            "contract_created" or "contract_completed" when notification.ContractId.HasValue => $"/Contracts/Details/{notification.ContractId}",
            "review_received" when notification.ReviewId.HasValue => $"/Reviews/Details/{notification.ReviewId}",
            _ => "/Notifications"
        };
    }
}