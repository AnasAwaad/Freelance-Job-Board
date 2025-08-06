using FreelanceJobBoard.Application.Features.Categories.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
public class UpdateCategoryCommand : IRequest<CategoryDto>
{
	[JsonIgnore]
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public string Description { get; set; } = null!;
}
