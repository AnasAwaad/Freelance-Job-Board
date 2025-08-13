using FreelanceJobBoard.Domain.Common;

namespace FreelanceJobBoard.Domain.Entities;
public class Review : BaseEntity
{
	public int Id { get; set; }
	public int JobId { get; set; }
	public string ReviewerId { get; set; }
	public string RevieweeId { get; set; }
	public int Rating { get; set; }
	public string Comment { get; set; }
	public string ReviewType { get; set; }
	public bool IsVisible { get; set; }
	public DateTime? CompletedAt { get; set; }
	public string? Tags { get; set; } // For additional tags like "On-time", "Good Communication", etc.
	public int? CommunicationRating { get; set; } // Additional rating dimension for communication (1-5)
	public int? QualityRating { get; set; } // Additional rating dimension for quality (1-5)
	public int? TimelinessRating { get; set; } // Additional rating dimension for timeliness (1-5)
	public bool WouldRecommend { get; set; } // Boolean for would recommend

	public Job Job { get; set; }
}

