using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Reports handled message consumer exceptions.
    /// </summary>
    public interface IConsumerExceptionReporter
    {
        /// <summary>
        /// Reports an exception descriptor.
        /// </summary>
        Task ReportAsync(
            ExceptionDescriptor descriptor,
            ConsumerExceptionContext context = null,
            CancellationToken cancellationToken = default);
    }
}
