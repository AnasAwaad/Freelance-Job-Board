namespace FreelanceJobBoard.Presentation.Models.ViewModels;

public class DashBoardViewModel
{
	public int? NumOfJobs { get; set; }
	public int? NumOfClients { get; set; }
	public int? NumOfFreelancers { get; set; }
	public IEnumerable<TopClientViewModel> TopClients { get; set; } = new List<TopClientViewModel>();
	public IEnumerable<JobListViewModel> RecentlyJobs { get; set; } = new List<JobListViewModel>();
}
