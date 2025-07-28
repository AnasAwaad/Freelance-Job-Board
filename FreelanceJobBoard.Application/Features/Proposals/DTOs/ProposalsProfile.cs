using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
internal class ProposalsProfile : Profile
{
	public ProposalsProfile()
	{
		CreateMap<CreateProposalCommand, Proposal>();

		CreateMap<Proposal, ProposalDto>()
			.ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
				src.Attachments.Select(pa => new AttachmentDto
				{
					Id = pa.Attachment.Id,
					FileName = pa.Attachment.FileName,
					Url = pa.Attachment.FilePath
				})));


	}
}
