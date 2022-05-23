using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Demo.Jobs;

[DisallowConcurrentExecution]
public sealed class ExpireMutationsJob : IJob, IDisposable
{
    private readonly ILogger<ExpireMutationsJob> _logger;
    private string _jobName;

    public ExpireMutationsJob(ILogger<ExpireMutationsJob> logger) => _logger = logger;

    public Task Execute(IJobExecutionContext context)
    {
        _jobName = context.JobDetail.Description;
        string lastRun = context.PreviousFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        _logger.LogInformation("Greetings from {jobName}! Previous run: {lastRun}", _jobName, lastRun);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _logger.LogDebug("Disposing {jobName}", _jobName);
    }
}
