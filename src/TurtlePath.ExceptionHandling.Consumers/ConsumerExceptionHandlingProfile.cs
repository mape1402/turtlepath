namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Base class for message consumer exception handling profiles.
    /// </summary>
    public abstract class ConsumerExceptionHandlingProfile : IConsumerExceptionHandlingProfile
    {
        /// <inheritdoc />
        public abstract void Configure(ConsumerExceptionHandlingOptionsBuilder builder);
    }
}
