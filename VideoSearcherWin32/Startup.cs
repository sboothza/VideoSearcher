using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace VideoSearcherWin32;

public class Startup
{
	// public Startup(IConfigurationRoot configuration)
	// {
	// 	Configuration = configuration;
	// }
	public Startup(IHostingEnvironment env)
	{
		var builder = new ConfigurationBuilder()
			.SetBasePath(env.ContentRootPath)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
			.AddEnvironmentVariables();
		Configuration = builder.Build();
	}

	public IConfigurationRoot Configuration { get; }

	public void ConfigureServices(IServiceCollection services)
	{
		// ...
	}

	// public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
	// {
	// 	// ...
	// }
}