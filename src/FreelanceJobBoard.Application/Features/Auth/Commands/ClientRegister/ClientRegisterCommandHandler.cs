using FreelanceJobBoard.Application.Interfaces;
using FreelanceJobBoard.Application.Interfaces.Services;
using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Application.Features.Auth.Commands.ClientRegister;
public class ClientRegisterCommandHandler(UserManager<ApplicationUser> userManager,
	RoleManager<IdentityRole> roleManager,
	IUnitOfWork unitOfWork,
	IEmailService emailService) : IRequestHandler<ClientRegisterCommand>

{

	public async Task Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
	{
		var user = new ApplicationUser
		{
			Email = request.Email,
			UserName = request.Email,
			FullName = request.FullName,
			ProfileImageUrl = request.ProfilePhotoUrl
		};

		var createResult = await userManager.CreateAsync(user, request.Password);
		if (!createResult.Succeeded)
		{
			var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
			throw new InvalidOperationException($"Failed to create client user: {errors}");
		}

		if (!await roleManager.RoleExistsAsync(AppRoles.Client))
		{
			await roleManager.CreateAsync(new IdentityRole(AppRoles.Client));
		}

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

		await unitOfWork.Clients.CreateAsync(client);
		await unitOfWork.SaveChangesAsync();


		// Send confirmation email
		//var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
		//var link = $"https://localhost:7117/Auth/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
		//await emailService.SendEmailAsync(request.Email, "Confirm your email", $"Click to confirm: <a href='{link}'>Confirm Email</a>");

	}
}