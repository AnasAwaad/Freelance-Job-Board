using FreelanceJobBoard.Application.Features.User.DTOs;
using MediatR;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetTopClients;
public class GetTopClientsQuery(int numOfClients) : IRequest<IEnumerable<TopClientDto>>
{
	public int NumOfClients { get; } = numOfClients;
}
