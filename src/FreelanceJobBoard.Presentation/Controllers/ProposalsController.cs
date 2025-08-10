using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceJobBoard.Presentation.Controllers;
public class ProposalsController : Controller
{
	private readonly ProposalService _proposalService;

	public ProposalsController(ProposalService appointmentService)
	{
		_proposalService = appointmentService;
	}

	[ValidateAntiForgeryToken]
	[HttpPost]
	public async Task<IActionResult> Create(CreateProposalViewModel dto)
	{
		if (!ModelState.IsValid)
		{
			return View();
		}

		var result = await _proposalService.CreateProposalAsync(dto);

		if (!result)
		{
			ModelState.AddModelError(string.Empty, "Error occurred while creating proposal");
			return PartialView("_Form", dto);
		}

		TempData["SuccessMessage"] = "Appointment created successfully.";
		return RedirectToAction("Index", "Home");
	}
}
