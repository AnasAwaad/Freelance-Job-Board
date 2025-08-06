namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class CategoryFormViewModel
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public string? Description { get; set; }
	public bool IsActive { get; set; }

}
