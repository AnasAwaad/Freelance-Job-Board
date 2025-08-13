using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetNumberOfJobs;
public class GetNumberOfJobsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetNumberOfJobsQuery, int>
{
	public async Task<int> Handle(GetNumberOfJobsQuery request, CancellationToken cancellationToken)
	{
		return await unitOfWork.Jobs.GetNumberOfJobs();
	}
}
