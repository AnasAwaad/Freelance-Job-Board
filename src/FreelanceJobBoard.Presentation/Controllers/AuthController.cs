using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreelanceJobBoard.Presentation.Controllers;
public class AuthController(AuthService authService) : Controller
{
	[Authorize(Roles = AppRoles.Client)]
	public IActionResult Index()
	{
		return View();
	}


	public IActionResult Login()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await authService.LoginAsync(viewModel);
		if (result is not null)
		{
			var signInResult = await SignInUserAsync(result, viewModel.RememberMe);
			if (signInResult)
			{
				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					return Redirect(returnUrl);

				return result.Role switch
				{
					"Client" => RedirectToAction("Index", "Clients"),
					"Freelancer" => RedirectToAction("Index", "Freelancers"),
					"Admin" => RedirectToAction("Index", "Admin"),
					_ => RedirectToAction("Index", "Home")
				};
			}
		}

		return View(viewModel);
	}

	private async Task<bool> SignInUserAsync(AuthResponseDto authResponse, bool rememberMe)
	{

		try
		{
			// prepare data will be in cookies
			List<Claim> claims = new()
			{
				new Claim(ClaimTypes.Name, authResponse.Email),
				new Claim(ClaimTypes.Role, authResponse.Role),
				new Claim("jwt", authResponse.Token)
			};

			claims.Add(new Claim("jwt", authResponse.Token));
			var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
			ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, scheme);
			AuthenticationProperties authenticationProperties = new AuthenticationProperties()
			{
				IsPersistent = rememberMe,
			};

			await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public IActionResult ClientRegister()
	{
		return View();
	}



	public IActionResult FreelancerRegister()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> FreelancerRegister(FreelancerRegisterViewModel viewModel)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await authService.FreelancerRegister(viewModel);
		if (result)
			return RedirectToAction("Login");

		return View(viewModel);
	}
}
