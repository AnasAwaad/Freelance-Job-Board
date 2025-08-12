using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Presentation.Models.DTOs;
using FreelanceJobBoard.Presentation.Models.ViewModels;
using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace FreelanceJobBoard.Presentation.Controllers;

public class AuthController : Controller
{
	private readonly AuthService _authService;
	private readonly UserService _userService;
	private readonly ILogger<AuthController> _logger;

	public AuthController(AuthService authService, UserService userService, ILogger<AuthController> logger)
	{
		_authService = authService;
		_userService = userService;
		_logger = logger;
	}

	public IActionResult Login()
	{
		// If user is already authenticated, redirect to dashboard
		if (User.Identity?.IsAuthenticated == true)
		{
			return RedirectToDashboard();
		}
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await _authService.LoginAsync(viewModel);
		if (result is not null)
		{
			var signInResult = await SignInUserAsync(result, viewModel.RememberMe);
			if (signInResult)
			{
				if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
					return Redirect(returnUrl);

				return RedirectToDashboard();
			}
		}

		ModelState.AddModelError("", "Invalid email or password.");
		return View(viewModel);
	}

	public IActionResult Register()
	{
		// If user is already authenticated, redirect to dashboard
		if (User.Identity?.IsAuthenticated == true)
		{
			return RedirectToDashboard();
		}
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(RegisterViewModel viewModel)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await _authService.RegisterAsync(viewModel);
		if (result)
		{
			TempData["Success"] = "Registration successful! Please check your email to confirm your account, then login.";
			return RedirectToAction("Login");
		}

		ModelState.AddModelError("", "Registration failed. Please try again.");
		return View(viewModel);
	}

	public IActionResult ForgotPassword()
	{
		if (User.Identity?.IsAuthenticated == true)
			return RedirectToDashboard();
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await _authService.ForgotPasswordAsync(viewModel);
		if (result)
		{
			TempData["Success"] = "Password reset instructions have been sent to your email address.";
			return RedirectToAction("ForgotPasswordConfirmation");
		}

		ModelState.AddModelError("", "Failed to send password reset email. Please try again.");
		return View(viewModel);
	}

	public IActionResult ForgotPasswordConfirmation()
	{
		return View();
	}

	public IActionResult ResetPassword(string? email, string? token)
	{
		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
		{
			TempData["Error"] = "Invalid password reset link.";
			return RedirectToAction("Login");
		}

		var viewModel = new ResetPasswordViewModel
		{
			Email = email,
			Token = token
		};

		return View(viewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
	{
		if (!ModelState.IsValid)
			return View(viewModel);

		var result = await _authService.ResetPasswordAsync(viewModel);
		if (result)
		{
			TempData["Success"] = "Your password has been reset successfully. You can now login with your new password.";
			return RedirectToAction("Login");
		}

		ModelState.AddModelError("", "Failed to reset password. Please try again or request a new reset link.");
		return View(viewModel);
	}

	public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
	{
		var viewModel = new ConfirmEmailViewModel();

		if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
		{
			viewModel.IsSuccess = false;
			viewModel.Message = "Invalid email confirmation link.";
			return View(viewModel);
		}

		var result = await _authService.ConfirmEmailAsync(userId, token);
		viewModel.UserId = userId;
		viewModel.Token = token;
		viewModel.IsSuccess = result;
		viewModel.Message = result ?
			"Your email has been confirmed successfully! You can now login to your account." :
			"Email confirmation failed. The link may be expired or invalid.";

		return View(viewModel);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> ResendEmailConfirmation(string email)
	{
		if (string.IsNullOrEmpty(email))
		{
			TempData["Error"] = "Email address is required.";
			return RedirectToAction("Login");
		}

		var result = await _authService.ResendEmailConfirmationAsync(email);
		if (result)
		{
			TempData["Success"] = "Confirmation email has been resent to your email address.";
		}
		else
		{
			TempData["Error"] = "Failed to resend confirmation email. Please try again.";
		}

		return RedirectToAction("Login");
	}

	private async Task<bool> SignInUserAsync(AuthResponseDto authResponse, bool rememberMe)
	{
		try
		{
			// Extract user ID from JWT token
			string? userId = null;
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
				userId = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value
						?? jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
						?? jwtToken.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to extract user ID from JWT token, attempting to get from user profile");
			}

			// If we couldn't get userId from token, try to get it from user profile
			if (string.IsNullOrEmpty(userId))
			{
				try
				{
					// Temporarily set up a minimal claims identity to make the API call
					var tempClaims = new List<Claim>
					{
						new Claim(ClaimTypes.Email, authResponse.Email),
						new Claim("jwt", authResponse.Token)
					};
					var tempIdentity = new ClaimsIdentity(tempClaims, CookieAuthenticationDefaults.AuthenticationScheme);
					HttpContext.User = new ClaimsPrincipal(tempIdentity);

					var userProfile = await _userService.GetCurrentUserProfileAsync();
					userId = userProfile?.UserProfile?.Id;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to get user profile for user ID extraction");
				}
			}

			if (string.IsNullOrEmpty(userId))
			{
				_logger.LogError("Unable to determine user ID for authentication");
				return false;
			}

			List<Claim> claims = new()
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Name, authResponse.FullName),
				new Claim(ClaimTypes.Email, authResponse.Email),
				new Claim(ClaimTypes.Role, authResponse.Role),
				new Claim("ProfileImageUrl", authResponse.ProfileImageUrl),
				new Claim("jwt", authResponse.Token)
			};

			var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
			ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, scheme);
			AuthenticationProperties authenticationProperties = new AuthenticationProperties()
			{
				IsPersistent = rememberMe,
				ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
			};

			await HttpContext.SignInAsync(scheme, new ClaimsPrincipal(claimsIdentity), authenticationProperties);

			_logger.LogInformation("User {Email} signed in successfully with UserId: {UserId}", authResponse.Email, userId);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during user sign in");
			return false;
		}
	}

	private IActionResult RedirectToDashboard()
	{
		var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
		return userRole switch
		{
			AppRoles.Admin => RedirectToAction("Index", "Admin"),
			AppRoles.Client => RedirectToAction("Index", "Home"),
			AppRoles.Freelancer => RedirectToAction("Index", "Home"),
			_ => RedirectToAction("Index", "Home")
		};
	}

	[HttpPost]
	[Authorize]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Logout()
	{
		try
		{
			_logger.LogInformation("User {UserEmail} is attempting to logout", User.Identity?.Name);

			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			// Clear session data if sessions are enabled
			try
			{
				HttpContext.Session?.Clear();
			}
			catch (Exception sessionEx)
			{
				_logger.LogWarning(sessionEx, "Failed to clear session during logout - sessions may not be configured");
			}

			_logger.LogInformation("User {UserEmail} logged out successfully", User.Identity?.Name);
			TempData["Success"] = "You have been successfully logged out.";
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during logout for user {UserEmail}", User.Identity?.Name);
			TempData["Error"] = "An error occurred during logout.";
		}

		return RedirectToAction("Login");
	}

	[HttpGet]
	[AllowAnonymous]
	public IActionResult JwtDebug()
	{
		try
		{
			var jwtToken = User?.FindFirst("jwt")?.Value;

			if (string.IsNullOrEmpty(jwtToken))
			{
				ViewBag.Error = "No JWT token found in claims";
				return View("Debug");
			}

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.ReadJwtToken(jwtToken);

			var tokenInfo = new
			{
				Header = token.Header,
				Payload = token.Claims.Select(c => new { c.Type, c.Value }).ToList(),
				ValidFrom = token.ValidFrom,
				ValidTo = token.ValidTo,
				RawToken = jwtToken
			};

			ViewBag.TokenInfo = tokenInfo;
			return View("Debug");
		}
		catch (Exception ex)
		{
			ViewBag.Error = $"Error parsing JWT: {ex.Message}";
			return View("Debug");
		}
	}

	// Quick debug action for testing authentication
	[HttpGet]
	[AllowAnonymous]
	public IActionResult TestAuth()
	{
		var claims = User?.Claims;
		object claimsList = null;

		if (claims != null)
		{
			claimsList = claims.Select(c => new { c.Type, c.Value }).ToList();
		}

		var debugInfo = new
		{
			IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
			AuthenticationType = User?.Identity?.AuthenticationType,
			Name = User?.Identity?.Name,
			Claims = claimsList,
			HasJwt = User?.FindFirst("jwt") != null
		};

		ViewBag.DebugInfo = debugInfo;
		return View("TestAuth");
	}

	public IActionResult AccessDenied()
	{
		return View();
	}


}
