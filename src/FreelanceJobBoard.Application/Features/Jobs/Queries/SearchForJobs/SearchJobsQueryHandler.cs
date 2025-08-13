using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.SearchForJobs;
internal class SearchJobsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<SearchJobsQuery, IEnumerable<JobSearchDto>>
{
	public async Task<IEnumerable<JobSearchDto>> Handle(SearchJobsQuery request, CancellationToken cancellationToken)
	{
		var result = await unitOfWork.Jobs.SearchJobsAsync(request.Query, request.Limit);

		return result;
	}
}
