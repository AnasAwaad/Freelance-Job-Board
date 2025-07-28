using FreelanceJobBoard.API.Attributes;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController(IMediator mediator) : ControllerBase
{
	[RateLimit(5, 60)]
	[HttpGet]
	public async Task<IActionResult> GetAll() =>
		 Ok(await mediator.Send(new GetAllCategoriesQuery()));


	[HttpGet("{id}")]
	public async Task<IActionResult> GetById([FromRoute] int id) =>
		Ok(await mediator.Send(new GetCategoryByIdQuery(id)));


	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
	{
		var id = await mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { id }, null);
	}


	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateCategoryCommand command)
	{
		command.Id = id;
		await mediator.Send(command);

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete([FromRoute] int id)
	{
		await mediator.Send(new DeleteCategoryCommand(id));

		return NoContent();
	}

}
