using FreelanceJobBoard.Application.Interfaces;
using MediatR;

namespace FreelanceJobBoard.Application.Features.User.Queries.GetNumberOfFreelancers;
internal class GetNumberOfFreelancersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetNumberOfFreelancersQuery, int>
{
	public async Task<int> Handle(GetNumberOfFreelancersQuery request, CancellationToken cancellationToken)
	{
		return await unitOfWork.Freelancers.GetTotalNumbers();
	}
}
