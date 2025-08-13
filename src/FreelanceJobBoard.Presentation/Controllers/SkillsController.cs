using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;

//[Authorize(Roles = AppRoles.Admin)]
public class SkillsController : Controller
{
    private readonly SkillService _skillService;

    public SkillsController(SkillService skillService)
    {
        _skillService = skillService;
    }

    public async Task<IActionResult> Index(string? search = null, bool? isActive = null)
    {
        var skills = await _skillService.GetAllSkillsAsync(search, isActive);
        
        ViewBag.Search = search;
        ViewBag.IsActive = isActive;
        
        return View(skills);
    }

    public async Task<IActionResult> Details(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        
        if (skill == null)
        {
            return NotFound();
        }

        return View(skill);
    }

    public IActionResult Create()
    {
        return View(new CreateSkillViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSkillViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var skillId = await _skillService.CreateSkillAsync(viewModel);
        
        if (skillId.HasValue)
        {
            TempData["Success"] = "Skill created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to create skill. The skill name might already exist.");
        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        
        if (skill == null)
        {
            return NotFound();
        }

        var viewModel = new UpdateSkillViewModel
        {
            Id = skill.Id,
            Name = skill.Name,
            IsActive = skill.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateSkillViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var success = await _skillService.UpdateSkillAsync(viewModel);
        
        if (success)
        {
            TempData["Success"] = "Skill updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to update skill. The skill name might already exist.");
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _skillService.DeleteSkillAsync(id);
        
        if (success)
        {
            TempData["Success"] = "Skill deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to delete skill. It might be referenced by existing jobs or freelancers.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        
        if (skill == null)
        {
            return NotFound();
        }

        return PartialView("_ConfirmDelete", skill);
    }

    // AJAX endpoint for creating skills from other forms
    [HttpPost]
    public async Task<IActionResult> CreateAjax([FromBody] CreateSkillViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var skillId = await _skillService.CreateSkillAsync(viewModel);
        
        if (skillId.HasValue)
        {
            var skill = await _skillService.GetSkillByIdAsync(skillId.Value);
            return Ok(skill);
        }

        return BadRequest("Failed to create skill");
    }

    // AJAX endpoint for getting skills for dropdowns
    [HttpGet]
    public async Task<IActionResult> GetSkillsJson(string? search = null, bool? isActive = true)
    {
        var skills = await _skillService.GetAllSkillsAsync(search, isActive);
        return Json(skills);
    }
}