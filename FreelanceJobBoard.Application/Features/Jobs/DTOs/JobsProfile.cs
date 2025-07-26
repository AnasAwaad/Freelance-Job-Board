using AutoMapper;
using FreelanceJobBoard.Application.Features.Jobs.Commands.CreateJob;
using FreelanceJobBoard.Application.Features.Jobs.Commands.UpdateJob;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
internal class JobsProfile : Profile
{
	public JobsProfile()
	{
		CreateMap<CreateJobCommand, Job>();
		CreateMap<UpdateJobCommand, Job>();
	}
}
