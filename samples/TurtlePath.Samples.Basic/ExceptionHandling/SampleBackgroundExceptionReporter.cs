using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.Workers;

namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleBackgroundExceptionReporter : IBackgroundExceptionReporter
{
    private readonly SampleExceptionReportLog reportLog;

    public SampleBackgroundExceptionReporter(SampleExceptionReportLog reportLog)
    {
        this.reportLog = reportLog;
    }

    public Task ReportAsync(
        ExceptionDescriptor descriptor,
        BackgroundExceptionContext context = null,
        CancellationToken cancellationToken = default)
    {
        reportLog.Add($"worker workload={context?.Workload}, kind={descriptor.Kind}, code={descriptor.Code}");

        return Task.CompletedTask;
    }
}
