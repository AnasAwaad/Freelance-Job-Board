using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetJobById;
internal class GetJobByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<GetJobByIdQuery, JobDto>
{
	public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
	{
		var job = await unitOfWork.Jobs.GetJobWithCategoriesAndSkillsAsync(request.Id);

		if (job is null)
			throw new NotFoundException(nameof(Job), request.Id.ToString());

		return mapper.Map<JobDto>(job);
	}
}
