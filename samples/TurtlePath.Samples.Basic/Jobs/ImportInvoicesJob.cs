using TurtlePath.Jobs;

namespace TurtlePath.Samples.Basic.Jobs;

public sealed class ImportInvoicesJob : TurtlePathJob
{
    private readonly SampleJobLog jobLog;

    public ImportInvoicesJob(SampleJobLog jobLog)
    {
        this.jobLog = jobLog;
    }

    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        jobLog.Add($"{context.JobName} started attempt {context.Attempt}");
        await Task.Delay(20, cancellationToken);
        jobLog.Add($"{context.JobName} completed");
    }
}
