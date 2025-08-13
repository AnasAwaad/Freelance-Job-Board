namespace FreelanceJobBoard.Application.Features.Dashboard.DTOs;

public class DashboardStatsDto
{
    public int ActiveJobs { get; set; }
    public int Proposals { get; set; }
    public int Contracts { get; set; }
    public int Reviews { get; set; }
    public int PendingApprovals { get; set; }
    public string UserRole { get; set; } = null!;
}

public class RecentActivityDto
{
    public List<ActivityItemDto> Activities { get; set; } = new();
}

public class ActivityItemDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = null!; // proposal, job, contract, review, etc.
    public string Icon { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? ActionUrl { get; set; }
}

public class ClientDashboardDto : DashboardStatsDto
{
    public int JobsPosted { get; set; }
    public int JobsPendingApproval { get; set; }
    public int ProposalsReceived { get; set; }
    public int ActiveContracts { get; set; }
    public int CompletedContracts { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal MonthlySpent { get; set; }
}

public class FreelancerDashboardDto : DashboardStatsDto
{
    public int ProposalsSubmitted { get; set; }
    public int ProposalsAccepted { get; set; }
    public int ActiveContracts { get; set; }
    public int CompletedContracts { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal MonthlyEarnings { get; set; }
    public double SuccessRate { get; set; }
    public double AverageRating { get; set; }
}

public class AdminDashboardDto : DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalFreelancers { get; set; }
    public int TotalClients { get; set; }
    public int JobsPendingApproval { get; set; }
    public int FlaggedJobs { get; set; }
    public int TotalJobsPosted { get; set; }
    public int TotalContractsCompleted { get; set; }
    public decimal PlatformRevenue { get; set; }
    public double AverageJobApprovalTime { get; set; }
}

public class EarningsAnalyticsDto
{
    public decimal TotalEarnings { get; set; }
    public decimal CurrentMonthEarnings { get; set; }
    public decimal LastMonthEarnings { get; set; }
    public List<MonthlyEarningsDto> MonthlyTrend { get; set; } = new();
    public List<CategoryEarningsDto> EarningsByCategory { get; set; } = new();
}

public class MonthlyEarningsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal Amount { get; set; }
    public int ContractsCompleted { get; set; }
}

public class CategoryEarningsDto
{
    public string CategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public int ProjectCount { get; set; }
}

public class SpendingAnalyticsDto
{
    public decimal TotalSpent { get; set; }
    public decimal CurrentMonthSpent { get; set; }
    public decimal LastMonthSpent { get; set; }
    public List<MonthlySpendingDto> MonthlyTrend { get; set; } = new();
    public List<CategorySpendingDto> SpendingByCategory { get; set; } = new();
}

public class MonthlySpendingDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public decimal Amount { get; set; }
    public int ProjectsCompleted { get; set; }
}

public class CategorySpendingDto
{
    public string CategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public int ProjectCount { get; set; }
}

public class JobApplicationFunnelDto
{
    public int JobsPosted { get; set; }
    public int ProposalsReceived { get; set; }
    public int ContractsSigned { get; set; }
    public int ProjectsCompleted { get; set; }
    public double ProposalConversionRate { get; set; }
    public double ContractConversionRate { get; set; }
    public double CompletionRate { get; set; }
}

public class ProposalSuccessRateDto
{
    public int ProposalsSubmitted { get; set; }
    public int ProposalsAccepted { get; set; }
    public double SuccessRate { get; set; }
    public List<MonthlySuccessRateDto> MonthlyTrend { get; set; } = new();
}

public class MonthlySuccessRateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public int Submitted { get; set; }
    public int Accepted { get; set; }
    public double Rate { get; set; }
}

public class ContractCompletionRateDto
{
    public int TotalContracts { get; set; }
    public int CompletedContracts { get; set; }
    public double CompletionRate { get; set; }
    public List<MonthlyCompletionRateDto> MonthlyTrend { get; set; } = new();
}

public class MonthlyCompletionRateDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public int Started { get; set; }
    public int Completed { get; set; }
    public double Rate { get; set; }
}

public class TopUsersDto
{
    public List<TopUserDto> Users { get; set; } = new();
}

public class TopUserDto
{
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public double Rating { get; set; }
    public int JobsCompleted { get; set; }
    public decimal TotalValue { get; set; }
    public string UserType { get; set; } = null!; // Freelancer or Client
}