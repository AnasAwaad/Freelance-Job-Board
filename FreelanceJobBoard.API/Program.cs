
using FreelanceJobBoard.API.Middlewares;
using FreelanceJobBoard.Application.Extensions;
using FreelanceJobBoard.Infrastructure.Extensions;

namespace FreelanceJobBoard.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            #region Add services to the container.

            builder.Services.AddScoped<ErrorHandlingMiddleware>();
			builder.Services.AddDistributedMemoryCache();
            //builder.Services.AddSingleton<RateLimitMiddleware>();

			builder.Services
				.AddApplication()
				.AddInfrastructure(builder.Configuration);


			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();


			#endregion

			var app = builder.Build();

            #region Configure the HTTP request pipeline.

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<RateLimitMiddleware>();

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
			#endregion
		}
	}
}
