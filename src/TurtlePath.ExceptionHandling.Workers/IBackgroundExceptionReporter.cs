using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Reports handled background workload exceptions.
    /// </summary>
    public interface IBackgroundExceptionReporter
    {
        /// <summary>
        /// Reports an exception descriptor.
        /// </summary>
        Task ReportAsync(
            ExceptionDescriptor descriptor,
            BackgroundExceptionContext context = null,
            CancellationToken cancellationToken = default);
    }
}
