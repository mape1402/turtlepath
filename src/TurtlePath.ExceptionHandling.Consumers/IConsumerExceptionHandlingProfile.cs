namespace TurtlePath.ExceptionHandling.Consumers
{
    /// <summary>
    /// Defines message consumer exception handling behavior.
    /// </summary>
    public interface IConsumerExceptionHandlingProfile
    {
        /// <summary>
        /// Configures message consumer exception handling.
        /// </summary>
        void Configure(ConsumerExceptionHandlingOptionsBuilder builder);
    }
}
