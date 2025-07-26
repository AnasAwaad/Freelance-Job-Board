using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
internal class ProposalsProfile : Profile
{
	public ProposalsProfile()
	{
		CreateMap<CreateProposalCommand, Proposal>();

	}
}
