using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace CoreWiki.Configuration.Startup;

public static partial class ConfigurationExtensions
{
	public static IApplicationBuilder ConfigureExceptions(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
			app.UseDatabaseErrorPage();
		}
		else
		{
			app.UseExceptionHandler("/Error");
		}

		return app;
	}
}
