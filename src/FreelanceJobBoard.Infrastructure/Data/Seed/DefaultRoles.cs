using FreelanceJobBoard.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace FreelanceJobBoard.Infrastructure.Data.Seed;
public static class DefaultRoles
{
	public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
	{
		if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
			await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));

		if (!await roleManager.RoleExistsAsync(AppRoles.Client))
			await roleManager.CreateAsync(new IdentityRole(AppRoles.Client));

		if (!await roleManager.RoleExistsAsync(AppRoles.Freelancer))
			await roleManager.CreateAsync(new IdentityRole(AppRoles.Freelancer));
	}
}