using FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
using FreelanceJobBoard.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController(IMediator mediator) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetAll() =>
		 Ok(await mediator.Send(new GetAllCategoriesQuery()));


	[HttpGet("{id}")]
	public async Task<IActionResult> GetById([FromRoute] int id) =>
		Ok(await mediator.Send(new GetCategoryByIdQuery(id)));
}
