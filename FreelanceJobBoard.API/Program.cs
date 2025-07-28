using FreelanceJobBoard.API.Middlewares;
using FreelanceJobBoard.Application.Extensions;
using FreelanceJobBoard.Infrastructure.Extensions;
using Serilog;

namespace FreelanceJobBoard.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
				.Build();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.Enrich.FromLogContext()
				.CreateLogger();

			try
			{
				Log.Information("Starting FreelanceJobBoard API");


				var builder = WebApplication.CreateBuilder(args);



				#region Add services to the container.

				builder.Logging.ClearProviders();
				builder.Logging.AddSerilog(Log.Logger);
				builder.Services.AddDistributedMemoryCache();
				
				builder.Services.AddHttpContextAccessor();
				
				builder.Services.AddScoped<ErrorHandlingMiddleware>();
				builder.Services.AddScoped<RequestResponseLoggingMiddleware>();

				builder.Services
					.AddApplication()
					.AddInfrastructure(builder.Configuration);

				builder.Services.AddControllers();
				builder.Services.AddEndpointsApiExplorer();
				builder.Services.AddSwaggerGen();

				#endregion

				var app = builder.Build();


				#region Configure the HTTP request pipeline.

				app.UseMiddleware<RequestResponseLoggingMiddleware>();

				app.UseMiddleware<ErrorHandlingMiddleware>();
				app.UseMiddleware<RateLimitMiddleware>();


				if (app.Environment.IsDevelopment())
				{
					app.UseDeveloperExceptionPage();
					app.UseSwagger();
					app.UseSwaggerUI();
				}

				app.UseHttpsRedirection();

				app.UseAuthorization();

				app.MapControllers();

				Log.Information("FreelanceJobBoard API started successfully");
				app.Run();

				#endregion
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
