using FreelanceJobBoard.API.Attributes;
using FreelanceJobBoard.Application.Features.Categories.Commands.CreateCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.DeleteCategory;
using FreelanceJobBoard.Application.Features.Categories.Commands.UpdateCategory;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetTopCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
//[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController(IMediator mediator) : ControllerBase
{
	[RateLimit(5, 60)]
	[HttpGet]
	public async Task<IActionResult> GetAll() =>
		 Ok(await mediator.Send(new GetAllCategoriesQuery()));

	[HttpGet("top/{numOfCategories}")]
	public async Task<IActionResult> GetTopCategories([FromRoute] int numOfCategories)
	{
		var result = await mediator.Send(new GetTopCategoriesQuery(numOfCategories));
		return Ok(result);
	}
	[HttpGet("{id}")]
	public async Task<IActionResult> GetById([FromRoute] int id) =>
		Ok(await mediator.Send(new GetCategoryByIdQuery(id)));


	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
	{
		var category = await mediator.Send(command);
		return CreatedAtAction(nameof(GetById), new { category.Id }, category);
	}


	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromRoute] int id, UpdateCategoryCommand command)
	{
		command.Id = id;
		var category = await mediator.Send(command);

		return Ok(category);
	}


	[HttpPost("{id}/ChangeStatus")]
	public async Task<IActionResult> ChangeStatus([FromRoute] int id)
	{
		var result = await mediator.Send(new ChangeCategoryStatusCommand(id));

		return Ok(result);
	}

}
