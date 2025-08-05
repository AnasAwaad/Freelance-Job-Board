using FreelanceJobBoard.Presentation.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FreelanceJobBoard.Presentation;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddControllersWithViews();
		builder.Services.AddHttpClient<AuthService>();

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		}).AddCookie(options =>
		{
			options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
			options.SlidingExpiration = true;

			options.LoginPath = "/Auth/Login";
			options.AccessDeniedPath = "/";
		});

		builder.Services.AddAuthorization();

		var app = builder.Build();


		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();

		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		app.Run();
	}
}
