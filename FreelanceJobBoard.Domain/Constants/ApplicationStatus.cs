namespace FreelanceJobBoard.Domain.Constants;

public static class JobStatus
{
    public const string Open = "Open";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Closed = "Closed";
}

public static class ProposalStatus
{
    public const string Submitted = "Submitted";
    public const string UnderReview = "Under Review";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Pending = "Pending";
}