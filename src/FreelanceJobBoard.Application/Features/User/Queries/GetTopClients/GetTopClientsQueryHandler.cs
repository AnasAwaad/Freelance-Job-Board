using AutoMapper;
using FreelanceJobBoard.Application.Features.User.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetTopClients;
internal class GetTopClientsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopClientsQuery, IEnumerable<TopClientDto>>
{
	public async Task<IEnumerable<TopClientDto>> Handle(GetTopClientsQuery request, CancellationToken cancellationToken)
	{
		return await unitOfWork.Clients.GetTopClientsAsync(request.NumOfClients);
	}
}
