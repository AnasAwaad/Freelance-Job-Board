using AutoMapper;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
using FreelanceJobBoard.Domain.Entities;

namespace FreelanceJobBoard.Application.Features.Categories.DTOs;
public class CategoriesProfile : Profile
{
	public CategoriesProfile()
	{
		CreateMap<Category, CategoryDto>();

		CreateMap<CreateCategoryCommand, Category>();
		CreateMap<UpdateCategoryCommand, Category>();
	}
}
