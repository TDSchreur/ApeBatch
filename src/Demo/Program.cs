using System.Diagnostics;
using Demo.Jobs;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Demo;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        LoggerConfiguration loggerBuilder = new LoggerConfiguration()
                                           .Enrich.FromLogContext()
                                           .MinimumLevel.Debug()
                                           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                           .MinimumLevel.Override("System", LogEventLevel.Information)
                                           .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                                            theme: AnsiConsoleTheme.Literate);

        Log.Logger = loggerBuilder.CreateLogger();

        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        try
        {
            IHost host = CreateHostBuilder(args).Build();

            await host.RunAsync()
                      .ContinueWith(_ => { })
                      .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, e.Message);
            Log.CloseAndFlush();
            await Task.Delay(1000);
            return -1;
        }

        return 0;
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        new HostBuilder()
           .UseContentRoot(Directory.GetCurrentDirectory())
           .UseSerilog((ctx, lc) =>
            {
                lc.WriteTo.Console()
                  .ReadFrom.Configuration(ctx.Configuration);
            })
           .ConfigureAppConfiguration((context, builder) =>
            {
                IHostEnvironment env = context.HostingEnvironment;

                builder.AddJsonFile("appsettings.json", false, false)
                       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                       .AddEnvironmentVariables()
                       .AddCommandLine(args);

                context.Configuration = builder.Build();
            })
           .UseDefaultServiceProvider((context, options) =>
            {
                bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                options.ValidateScopes = isDevelopment;
                options.ValidateOnBuild = isDevelopment;
            })
           .ConfigureServices((hostContext, services) =>
            {
                services.AddOptions();
                services.Configure<QuartzOptions>(hostContext.Configuration.GetSection("Quartz"));

                services.AddQuartz(q =>
                {
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();

                    q.ScheduleJob<ExpireMutationsJob>(triggerConfigurator =>
                                                      {
                                                          triggerConfigurator.WithIdentity("ExpireTrigger");
                                                          triggerConfigurator.WithCronSchedule("0/10 * * * * ?");
                                                      },
                                                      jobConfigurator =>
                                                      {
                                                          jobConfigurator.WithIdentity("ExpireJob");
                                                          jobConfigurator.WithDescription("Expire unapproved mutations");
                                                      });
                });

                services.AddQuartzHostedService(options =>
                {
                    options.WaitForJobsToComplete = true;
                });

                services.AddTransient<ExpireMutationsJob>();
            });
}
