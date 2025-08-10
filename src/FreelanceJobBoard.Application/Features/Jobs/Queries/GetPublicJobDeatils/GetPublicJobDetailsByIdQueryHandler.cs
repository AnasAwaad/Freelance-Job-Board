using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelanceJobBoard.Application.Features.Jobs.Queries.GetPublicJobDeatils;
internal class GetPublicJobDetailsByIdQueryHandler(IUnitOfWork unitOfWork,
	IMapper mapper) : IRequestHandler<GetPublicJobDetailsByIdQuery, PublicJobDetailsDto>
{
	public async Task<PublicJobDetailsDto?> Handle(GetPublicJobDetailsByIdQuery request, CancellationToken cancellationToken)
	{
		var job = unitOfWork.Jobs.getPublicJobDetails(request.JobId);

		return await mapper.ProjectTo<PublicJobDetailsDto>(job).FirstOrDefaultAsync();
	}
}
