using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetNumberOfClients;
internal class GetNumberOfClientsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetNumberOfClientsQuery, int>
{
	public async Task<int> Handle(GetNumberOfClientsQuery request, CancellationToken cancellationToken)
	{
		return await unitOfWork.Clients.GetTotalNumbers();
	}
}
