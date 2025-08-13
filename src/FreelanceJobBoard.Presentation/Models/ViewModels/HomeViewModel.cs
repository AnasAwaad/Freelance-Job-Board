namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class HomeViewModel
{
	public IEnumerable<JobListViewModel>? RecentJobs { get; set; }
	public IEnumerable<PublicCategoryViewModel> TopCategories { get; set; }

}
