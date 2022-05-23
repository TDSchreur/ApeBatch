using System.Globalization;
using Quartz;

namespace Demo.Jobs;

[DisallowConcurrentExecution]
public class ExpireMutationsJob : IJob, IDisposable
{
    private readonly ILogger<ExpireMutationsJob> _logger;
    private string _jobName;

    public ExpireMutationsJob(ILogger<ExpireMutationsJob> logger) => _logger = logger;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public Task Execute(IJobExecutionContext context)
    {
        _jobName = context.JobDetail.Description;
        string lastRun = context.PreviousFireTimeUtc?.DateTime.ToLocalTime().ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _logger.LogInformation("Greetings from {jobName}! Previous run: {lastRun}", _jobName, lastRun);

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _logger.LogDebug("Disposing {jobName}", _jobName);
        }
    }
}
