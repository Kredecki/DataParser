using DataParser.Infrastructure;
using DataParser.Infrastructure.Abstractions.Converters;
using DataParser.Infrastructure.Abstractions.Services;
using DataParser.Infrastructure.Abstractions.Strategies.Parse;
using DataParser.Infrastructure.Abstractions.Validators;
using DataParser.Infrastructure.Converters;
using DataParser.Infrastructure.Services;
using DataParser.Infrastructure.Strategies.ParseStrategy;
using DataParser.Infrastructure.Validators;

namespace DataParser.API;

public static class Application
{
	public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
	{
		#region MediatR
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ICommandQuery).Assembly));
		#endregion

		#region Cors
		services.AddCors(options =>
		{
			options.AddPolicy("AllowedOrigins",
				builder => builder.WithOrigins(configuration["AllowedOrigin"]!)
								  .AllowAnyHeader()
								  .AllowAnyMethod());
		});
		#endregion

		#region Services
		services.AddSingleton<ITokenService, TokenService>();
		#endregion

		#region Strategies
		services.AddScoped<IParseStrategy, CSVParser>();
		services.AddScoped<IParseStrategy, InternalJsonParser>();
		services.AddScoped<IParseStrategyResolver, ParseStrategyResolver>();
		#endregion

		#region Validators
		services.AddSingleton<IInternalJsonParserValidator, InternalJsonParserValidator>();
		#endregion

		#region Converters
		services.AddSingleton<IInternalJsonParserConverter, InternalJsonParserConverter>();
		services.AddSingleton<ICSVParserConverter, CSVParserConverter>();
		#endregion

		return services;
	}
}
