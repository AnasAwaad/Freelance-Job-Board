using FreelanceJobBoard.Application.DTOs;
using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using FreelanceJobBoard.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Infrastructure.Services;
public class AuthService : IAuthService
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	private readonly IJwtTokenGenerator _jwtTokenGenerator;
	private readonly ApplicationDbContext _dbContext;
	private readonly IEmailService _emailService;


	public AuthService(UserManager<ApplicationUser> userManager,
					   SignInManager<ApplicationUser> signInManager,
					   RoleManager<IdentityRole> roleManager,
					   IJwtTokenGenerator jwtTokenGenerator,
					   ApplicationDbContext dbContext,
					   IEmailService emailService)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_roleManager = roleManager;
		_jwtTokenGenerator = jwtTokenGenerator;
		_dbContext = dbContext;
		_emailService = emailService;
	}

	public async Task<AuthResponseDto> LoginAsync(string email, string password)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null)
			throw new UnauthorizedAccessException("Invalid email or password.");

		var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
		if (!result.Succeeded)
			throw new UnauthorizedAccessException("Invalid email or password.");

		var roles = await _userManager.GetRolesAsync(user);
		var role = roles.FirstOrDefault() ?? "User";

		var token = _jwtTokenGenerator.GenerateToken(user, role);

		return new AuthResponseDto
		{
			Token = token,
			Email = user.Email!,
			Role = role
		};
	}

	public async Task RegisterClientAsync(string email, string password, string fullName)
	{
		var user = new ApplicationUser
		{
			Email = email,
			UserName = email,
			FullName = fullName
		};

		var createResult = await _userManager.CreateAsync(user, password);
		if (!createResult.Succeeded)
		{
			var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
			throw new InvalidOperationException($"Failed to create client user: {errors}");
		}

		if (!await _roleManager.RoleExistsAsync(AppRoles.Client))
		{
			await _roleManager.CreateAsync(new IdentityRole(AppRoles.Client));
		}

		await _userManager.AddToRoleAsync(user, AppRoles.Client);

		var client = new Client
		{
			UserId = user.Id,
			Company = new Company(),
			AverageRating = 0,
			TotalReviews = 0,
			IsActive = true,
			CreatedOn = DateTime.UtcNow
		};

		_dbContext.Clients.Add(client);
		await _dbContext.SaveChangesAsync();

		// Send confirmation email
		//var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		//var link = $"https://your-frontend.com/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
		//await _emailService.SendEmailAsync(email, "Confirm your email", $"Click to confirm: <a href='{link}'>Confirm Email</a>");
	}


	public async Task RegisterFreelancerAsync(string email, string password, string fullName)
	{
		var user = new ApplicationUser
		{
			Email = email,
			UserName = email,
			FullName = fullName
		};

		var createResult = await _userManager.CreateAsync(user, password);
		if (!createResult.Succeeded)
			throw new InvalidOperationException("Failed to create freelancer user.");

		if (!await _roleManager.RoleExistsAsync(AppRoles.Freelancer))
		{
			await _roleManager.CreateAsync(new IdentityRole(AppRoles.Freelancer));
		}

		await _userManager.AddToRoleAsync(user, AppRoles.Freelancer);

		var freelancer = new Freelancer
		{
			UserId = user.Id,
			Bio = string.Empty,
			AverageRating = 0,
			TotalReviews = 0,
			IsActive = true,
			CreatedOn = DateTime.UtcNow
		};

		_dbContext.Freelancers.Add(freelancer);
		await _dbContext.SaveChangesAsync();

		// Send confirmation email
		//var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
		//var link = $"https://your-frontend.com/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
		//await _emailService.SendEmailAsync(email, "Confirm your email", $"Click to confirm: <a href='{link}'>Confirm Email</a>");
	}

}