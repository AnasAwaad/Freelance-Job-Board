using AutoMapper;
using FreelanceJobBoard.Application.Features.Admin.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
internal class GetJobByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<GetJobByIdQuery, JobDetailsDto>
{
	public async Task<JobDetailsDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
	{
		var jobQuery = unitOfWork.Jobs.GetJobWithDetailsQuery(request.Id);

		var job = await mapper.ProjectTo<JobDetailsDto>(jobQuery).FirstOrDefaultAsync();

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());

		return mapper.Map<JobDetailsDto>(job);
	}
}
