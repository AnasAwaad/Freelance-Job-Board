using FreelanceJobBoard.Application.Features.Categories.Queries.GetAllCategories;
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



}
