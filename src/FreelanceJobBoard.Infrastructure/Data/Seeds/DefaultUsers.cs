using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Infrastructure.Data.Seeds;

public static class DefaultUsers
{
	public static async Task SeedUsers(UserManager<ApplicationUser> userManager)
	{
		var admin = new ApplicationUser
		{
			FullName = "Admin",
			Email = "Admin@gmail.com",
			UserName = "Admin",
			EmailConfirmed = true,
		};

		var client = new ApplicationUser
		{
			FullName = "Customer",
			Email = "Client@gmail.com",
			UserName = "Client",
			EmailConfirmed = true,
		};

		var freelancer = new ApplicationUser
		{
			FullName = "Freelancer",
			Email = "Freelancer@gmail.com",
			UserName = "Freelancer",
			EmailConfirmed = true,
		};

		var adminUser = await userManager.FindByNameAsync(admin.UserName);
		var clientUser = await userManager.FindByNameAsync(client.UserName);
		var freelancerUser = await userManager.FindByNameAsync(freelancer.UserName);

		if (adminUser is null)
		{
			await userManager.CreateAsync(admin, "Pa$$w0rd");
			await userManager.AddToRoleAsync(admin, AppRoles.Admin);
		}

		if (clientUser is null)
		{
			await userManager.CreateAsync(client, "Pa$$w0rd");
			await userManager.AddToRoleAsync(client, AppRoles.Client);
		}


		if (freelancerUser is null)
		{
			await userManager.CreateAsync(freelancer, "Pa$$w0rd");
			await userManager.AddToRoleAsync(freelancer, AppRoles.Freelancer);
		}
	}
}
