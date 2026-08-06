using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.Consumers;

namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleConsumerExceptionReporter : IConsumerExceptionReporter
{
    private readonly SampleExceptionReportLog reportLog;

    public SampleConsumerExceptionReporter(SampleExceptionReportLog reportLog)
    {
        this.reportLog = reportLog;
    }

    public Task ReportAsync(
        ExceptionDescriptor descriptor,
        ConsumerExceptionContext context = null,
        CancellationToken cancellationToken = default)
    {
        reportLog.Add($"consumer message={context?.MessageName}, id={context?.MessageId}, kind={descriptor.Kind}, code={descriptor.Code}");

        return Task.CompletedTask;
    }
}
