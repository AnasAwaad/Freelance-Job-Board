using AutoMapper;
using FreelanceJobBoard.Application.Features.Proposals.Commands.CreateProposal;
using FreelanceJobBoard.Application.Features.Proposals.DTOs;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Proposals.DTOs;
public class ProposalsProfile : Profile
{
	public ProposalsProfile()
	{
		CreateMap<CreateProposalCommand, Proposal>();

		CreateMap<Proposal, ProposalDto>()
			.ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
			.ForMember(dest => dest.JobDescription, opt => opt.MapFrom(src => src.Job.Description))
			.ForMember(dest => dest.JobBudgetMin, opt => opt.MapFrom(src => src.Job.BudgetMin))
			.ForMember(dest => dest.JobBudgetMax, opt => opt.MapFrom(src => src.Job.BudgetMax))
			.ForMember(dest => dest.JobDeadline, opt => opt.MapFrom(src => src.Job.Deadline))
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Job != null && src.Job.Client != null ? src.Job.Client.User.FullName : null))
			.ForMember(dest => dest.ClientProfileImageUrl, opt => opt.MapFrom(src => src.Job != null && src.Job.Client != null ? src.Job.Client.User.ProfileImageUrl : null))
			.ForMember(dest => dest.ClientAverageRating, opt => opt.MapFrom(src => src.Job != null && src.Job.Client != null ? src.Job.Client.AverageRating : 0))
			.ForMember(dest => dest.ClientTotalReviews, opt => opt.MapFrom(src => src.Job != null && src.Job.Client != null ? src.Job.Client.TotalReviews : 0))
			.ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer != null ? src.Freelancer.User.FullName : null))
			.ForMember(dest => dest.FreelancerProfileImageUrl, opt => opt.MapFrom(src => src.Freelancer != null ? src.Freelancer.User.ProfileImageUrl : null))
			.ForMember(dest => dest.FreelancerAverageRating, opt => opt.MapFrom(src => src.Freelancer != null ? src.Freelancer.AverageRating : 0))
			.ForMember(dest => dest.FreelancerTotalReviews, opt => opt.MapFrom(src => src.Freelancer != null ? src.Freelancer.TotalReviews : 0))
			.ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
				src.Attachments.Select(pa => new AttachmentDto
				{
					Id = pa.Attachment.Id,
					FileName = pa.Attachment.FileName,
					FilePath = pa.Attachment.FilePath
				})));

		CreateMap<Proposal, ProposalWithDetailsDto>()
			.ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
				src.Client!.User!.FullName))
			.ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src =>
				src.Client!.Company.Name))
			.ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src =>
				src.Job.Title))
			.ForMember(dest => dest.JobDescription, opt => opt.MapFrom(src =>
				src.Job.Description))
			.ForMember(dest => dest.JobDeadline, opt => opt.MapFrom(src =>
				src.Job.Deadline))
			.ForMember(dest => dest.JobStatus, opt => opt.MapFrom(src =>
				src.Job.Status))
			.ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
				src.Attachments.Select(pa => new AttachmentDto
				{
					Id = pa.Attachment.Id,
					FileName = pa.Attachment.FileName,
					FilePath = pa.Attachment.FilePath
				})));
	}
}
