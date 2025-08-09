namespace FreelanceJobBoard.Presentation.Models.DTOs;

public class ChangeCategoryStatusResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool NewStatus { get; set; }
    public int CategoryId { get; set; }
}
