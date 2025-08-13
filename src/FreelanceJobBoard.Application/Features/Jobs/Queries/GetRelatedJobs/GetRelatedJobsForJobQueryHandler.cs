using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetRelatedJobs;
internal class GetRelatedJobsForJobQueryHandler(IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<GetRetatedJobsForJobQuery, IEnumerable<PublicJobListDto>>
{
	public async Task<IEnumerable<PublicJobListDto>> Handle(GetRetatedJobsForJobQuery request, CancellationToken cancellationToken)
	{
		var query = unitOfWork.Jobs.GetRelatedJobsQueryable(request.Id);

		return await mapper.ProjectTo<PublicJobListDto>(query).ToListAsync();
	}
}
