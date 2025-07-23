using AutoMapper;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
internal class CategoriesProfile : Profile
{
	public CategoriesProfile()
	{
		CreateMap<Category, CategoryDto>();
	}
}
