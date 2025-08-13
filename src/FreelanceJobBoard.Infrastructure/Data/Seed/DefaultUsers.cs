using FreelanceJobBoard.Domain.Constants;
using FreelanceJobBoard.Domain.Entities;
using FreelanceJobBoard.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Infrastructure.Data.Seed;

public static class DefaultUsers
{
	public static async Task SeedUsers(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
	{
		var admin = new ApplicationUser
		{
			FullName = "Admin",
			Email = "Admin@gmail.com",
			UserName = "Admin",
			EmailConfirmed = true,
		};

		var client = new Client
		{
			AverageRating = 0,
			TotalReviews = 0,
			User = new ApplicationUser
			{
				FullName = "Client",
				Email = "Client@gmail.com",
				UserName = "Client",
				EmailConfirmed = true,
			}
		};


		var freelancer = new Freelancer
		{
			Bio = "test",
			User = new ApplicationUser
			{
				FullName = "Freelancer",
				Email = "Freelancer@gmail.com",
				UserName = "Freelancer",
				EmailConfirmed = true,
			}
		};

		var adminUser = await userManager.FindByNameAsync(admin.UserName);
		var clientUser = await userManager.FindByNameAsync(client.User.UserName);
		var freelancerUser = await userManager.FindByNameAsync(freelancer.User.UserName);

		if (adminUser is null)
		{
			await userManager.CreateAsync(admin, "Pa$$w0rd");
			await userManager.AddToRoleAsync(admin, AppRoles.Admin);
		}

		if (clientUser is null)
		{
			await userManager.CreateAsync(client.User, "Pa$$w0rd");
			await userManager.AddToRoleAsync(client.User, AppRoles.Client);


			dbContext.Clients.Add(client);
		}


		if (freelancerUser is null)
		{
			await userManager.CreateAsync(freelancer.User, "Pa$$w0rd");
			await userManager.AddToRoleAsync(freelancer.User, AppRoles.Freelancer);
			dbContext.Freelancers.Add(freelancer);

		}
	}
}