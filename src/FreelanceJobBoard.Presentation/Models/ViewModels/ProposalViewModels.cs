using System.ComponentModel.DataAnnotations;

namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class ProposalViewModel
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string? JobTitle { get; set; }
    public string? CoverLetter { get; set; }
    public decimal BidAmount { get; set; }
    public int EstimatedTimelineDays { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime? ReviewedAt { get; set; }
    public string? ClientFeedback { get; set; }
    public List<AttachmentViewModel> Attachments { get; set; } = new();
    
    // Job related properties for display
    public string? JobDescription { get; set; }
    public decimal JobBudgetMin { get; set; }
    public decimal JobBudgetMax { get; set; }
    public DateTime JobDeadline { get; set; }
    public string? ClientName { get; set; }
    
    // Freelancer information (for JobProposals view - when client views proposals)
    public string? FreelancerName { get; set; }
    public string? FreelancerProfileImageUrl { get; set; }
    public decimal FreelancerAverageRating { get; set; }
    public int FreelancerTotalReviews { get; set; }
    
    // Client information (for freelancer proposal details)
    public string? ClientProfileImageUrl { get; set; }
    public decimal ClientAverageRating { get; set; }
    public int ClientTotalReviews { get; set; }
}

public class SubmitProposalViewModel
{
    public int JobId { get; set; }
    
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = "";
    
    [Display(Name = "Job Description")]
    public string JobDescription { get; set; } = "";
    
    [Display(Name = "Budget Range")]
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    
    [Display(Name = "Deadline")]
    public DateTime Deadline { get; set; }

    [Required(ErrorMessage = "Cover letter is required")]
    [StringLength(2000, ErrorMessage = "Cover letter cannot exceed 2000 characters")]
    [Display(Name = "Cover Letter")]
    public string CoverLetter { get; set; } = "";

    [Required(ErrorMessage = "Bid amount is required")]
    [Range(1, 1000000, ErrorMessage = "Bid amount must be between $1 and $1,000,000")]
    [Display(Name = "Your Bid Amount ($)")]
    public decimal BidAmount { get; set; }

    [Required(ErrorMessage = "Estimated timeline is required")]
    [Range(1, 365, ErrorMessage = "Timeline must be between 1 and 365 days")]
    [Display(Name = "Estimated Timeline (Days)")]
    public int EstimatedTimelineDays { get; set; }

    [Display(Name = "Portfolio Files (Optional)")]
    public List<IFormFile>? PortfolioFiles { get; set; }
    
    // Client rating information for display in submission view
    public decimal ClientAverageRating { get; set; }
    public int ClientTotalReviews { get; set; }
}

public class AttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public long FileSize { get; set; }
    public string ContentType { get; set; } = "";
}