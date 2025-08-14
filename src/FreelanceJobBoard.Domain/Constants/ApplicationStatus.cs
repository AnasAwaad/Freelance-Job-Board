namespace FreelanceJobBoard.Domain.Constants;

public static class JobStatus
{
	public const string Pending = "Pending";
	public const string Open = "Open";
	public const string InProgress = "In Progress";
	public const string Completed = "Completed";
	public const string Cancelled = "Cancelled";
	public const string Closed = "Closed";
}

public static class ProposalStatus
{
	public const string Submitted = "Submitted";
	public const string UnderReview = "UnderReview";
	public const string Accepted = "Accepted";
	public const string Rejected = "Rejected";
	public const string Pending = "Pending";
	public const string Withdrawn = "Withdrawn";
	public const string Expired = "Expired";

	public static readonly string[] AllStatuses = 
	{
		Submitted,
		Pending,
		UnderReview,
		Accepted,
		Rejected,
		Withdrawn,
		Expired
	};

	public static readonly string[] ActiveStatuses = 
	{
		Submitted,
		Pending,
		UnderReview
	};

	public static readonly string[] CompletedStatuses = 
	{
		Accepted,
		Rejected,
		Withdrawn,
		Expired
	};

	public static bool IsValidStatus(string status)
	{
		return AllStatuses.Contains(status);
	}

	public static bool IsActiveStatus(string status)
	{
		return ActiveStatuses.Contains(status);
	}

	public static bool IsCompletedStatus(string status)
	{
		return CompletedStatuses.Contains(status);
	}
}

public static class ContractStatus
{
	public const string Pending = "Pending";
	public const string Active = "Active";
	public const string PendingApproval = "Pending Approval";
	public const string Completed = "Completed";
	public const string Cancelled = "Cancelled";
}

public static class ContractChangeRequestStatus
{
	public const string Pending = "Pending";
	public const string Approved = "Approved";
	public const string Rejected = "Rejected";
	public const string Expired = "Expired";
}

public static class ReviewType
{
	public const string ClientToFreelancer = "ClientToFreelancer";
	public const string FreelancerToClient = "FreelancerToClient";
}

public static class UserRole
{
	public const string Client = "Client";
	public const string Freelancer = "Freelancer";
}

