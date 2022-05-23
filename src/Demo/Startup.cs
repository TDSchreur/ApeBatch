using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Demo;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health/readiness");

            endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions
            {
                Predicate = _ => false
            });
        });

        app.Run(async context => { await context.Response.WriteAsync($"{Environment.MachineName}: Hello world! Request path: {context.Request.Path}"); });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRouting();

        services.AddHealthChecks();
    }
}
