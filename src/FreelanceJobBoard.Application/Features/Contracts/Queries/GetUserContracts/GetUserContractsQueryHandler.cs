using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Exceptions;
using MediatR;

namespace FreelanceJobBoard.Application.Features.Contracts.Queries.GetUserContracts;

public class GetUserContractsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : IRequestHandler<GetUserContractsQuery, GetUserContractsResult>
{
    public async Task<GetUserContractsResult> Handle(GetUserContractsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User must be authenticated to view contracts");

        var userId = request.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            userId = currentUserService.UserId!;
        }

        var contracts = new List<Contract>();

        // Check if user is a client
        var client = await unitOfWork.Clients.GetByUserIdAsync(userId);
        if (client != null)
        {
            contracts.AddRange(await unitOfWork.Contracts.GetContractsByClientIdAsync(client.Id));
        }

        // Check if user is a freelancer
        var freelancer = await unitOfWork.Freelancers.GetByUserIdAsync(userId);
        if (freelancer != null)
        {
            contracts.AddRange(await unitOfWork.Contracts.GetContractsByFreelancerIdAsync(freelancer.Id));
        }

        if (client == null && freelancer == null)
        {
            throw new NotFoundException("User profile", userId);
        }

        var contractDtos = contracts.Select(c => new ContractDto
        {
            Id = c.Id,
            ProposalId = c.ProposalId,
            JobTitle = c.Proposal?.Job?.Title ?? "Unknown Job",
            ClientName = c.Client?.User?.FullName ?? "Unknown Client",
            FreelancerName = c.Freelancer?.User?.FullName ?? "Unknown Freelancer",
            PaymentAmount = c.PaymentAmount,
            AgreedPaymentType = c.AgreedPaymentType,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            ContractStatus = c.ContractStatus?.Name ?? "Unknown",
            CreatedOn = c.CreatedOn,
            LastUpdatedOn = c.LastUpdatedOn
        }).ToList();

        return new GetUserContractsResult
        {
            Contracts = contractDtos
        };
    }
}