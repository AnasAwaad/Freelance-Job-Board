using FreelanceJobBoard.Application.Features.Skills.Commands.CreateSkill;
using FreelanceJobBoard.Application.Features.Skills.Commands.DeleteSkill;
using FreelanceJobBoard.Application.Features.Skills.Commands.UpdateSkill;
using FreelanceJobBoard.Application.Features.Skills.Queries.GetAllSkills;
using FreelanceJobBoard.Application.Features.Skills.Queries.GetSkillById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SkillsController(IMediator mediator) : ControllerBase
{
   
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllSkillsQuery query)
    {
        var skills = await mediator.Send(query);
        return Ok(skills);
    }

   
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var skill = await mediator.Send(new GetSkillByIdQuery(id));
        return Ok(skill);
    }

 
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSkillCommand command)
    {
        var skillId = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = skillId }, null);
    }

   
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSkillCommand command)
    {
        command.Id = id;
        await mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await mediator.Send(new DeleteSkillCommand(id));
        return NoContent();
    }
}