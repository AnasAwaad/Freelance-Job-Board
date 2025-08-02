using AutoMapper;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Admin.Queries.GetAllJobs;
internal class GetAllJobsWithStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAllJobsWithStatusQuery, IEnumerable<JobListDto>>
{
	public async Task<IEnumerable<JobListDto>> Handle(GetAllJobsWithStatusQuery request, CancellationToken cancellationToken)
	{

		var jobs = await mapper
				.ProjectTo<JobListDto>(unitOfWork.Jobs.GetAllWithClientQueryable(request.Status))
				.ToListAsync();

		return jobs;
	}
}
