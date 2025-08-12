using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
public class ClientRegisterCommandHandler(UserManager<ApplicationUser> userManager,
	RoleManager<IdentityRole> roleManager,
	IUnitOfWork unitOfWork,
	IEmailService emailService,
	ILogger<ClientRegisterCommandHandler> logger) : IRequestHandler<ClientRegisterCommand>

{

	public async Task Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
	{
		logger.LogInformation("🆕 Starting client registration | Email={Email}, CompanyName={CompanyName}", 
			request.Email, request.CompanyName ?? "No company");

		try
		{
			var user = new ApplicationUser
			{
				Email = request.Email,
				UserName = request.Email,
				FullName = request.FullName,
				ProfileImageUrl = request.ProfilePhotoUrl
			};

			logger.LogDebug("👤 Creating application user | Email={Email}, FullName={FullName}", 
				request.Email, request.FullName);

			var createResult = await userManager.CreateAsync(user, request.Password);
			if (!createResult.Succeeded)
			{
				var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
				logger.LogError("❌ Failed to create application user | Email={Email}, Errors={Errors}", 
					request.Email, errors);
				throw new InvalidOperationException($"Failed to create client user: {errors}");
			}

			logger.LogDebug("✅ Application user created successfully | UserId={UserId}, Email={Email}", 
				user.Id, request.Email);

			// Ensure client role exists
			if (!await roleManager.RoleExistsAsync(AppRoles.Client))
			{
				logger.LogDebug("🔧 Creating Client role as it doesn't exist");
				await roleManager.CreateAsync(new IdentityRole(AppRoles.Client));
				logger.LogDebug("✅ Client role created successfully");
			}

			logger.LogDebug("🔑 Assigning Client role to user | UserId={UserId}", user.Id);
			await userManager.AddToRoleAsync(user, AppRoles.Client);

			var client = new Client
			{
				UserId = user.Id,
				Company = new Company
				{
					Name = request.CompanyName,
					WebsiteUrl = request.CompanyWebsite,
					Industry = request.Industry
				},
			};

			logger.LogDebug("🏢 Creating client profile | UserId={UserId}, CompanyName={CompanyName}, Industry={Industry}", 
				user.Id, request.CompanyName ?? "N/A", request.Industry ?? "N/A");

			await unitOfWork.Clients.CreateAsync(client);
			await unitOfWork.SaveChangesAsync();

			logger.LogInformation("✅ Client registration completed successfully | UserId={UserId}, ClientId={ClientId}, Email={Email}", 
				user.Id, client.Id, request.Email);

			// Send confirmation email
			//var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
			//var link = $"https://localhost:7117/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
			//await emailService.SendEmailAsync(request.Email, "Confirm your email", $"Click to confirm: <a href='{link}'>Confirm Email</a>");
			logger.LogDebug("📧 Email confirmation temporarily disabled for development");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "❌ Client registration failed | Email={Email}", request.Email);
			throw;
		}
	}
}