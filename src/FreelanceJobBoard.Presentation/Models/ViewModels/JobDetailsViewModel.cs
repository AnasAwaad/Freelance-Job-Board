namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class JobDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BudgetMin { get; set; }
    public decimal BudgetMax { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}