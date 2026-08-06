using TurtlePath.Jobs;

namespace TurtlePath.Samples.Basic.Jobs;

public sealed class RefreshCatalogJob : TurtlePathJob
{
    private readonly SampleJobLog jobLog;

    public RefreshCatalogJob(SampleJobLog jobLog)
    {
        this.jobLog = jobLog;
    }

    public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        jobLog.Add($"{context.JobName} refreshed catalog on attempt {context.Attempt}");

        return Task.CompletedTask;
    }
}
