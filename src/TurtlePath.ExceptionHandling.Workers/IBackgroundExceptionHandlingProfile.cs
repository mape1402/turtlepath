namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Defines background worker exception handling behavior.
    /// </summary>
    public interface IBackgroundExceptionHandlingProfile
    {
        /// <summary>
        /// Configures background worker exception handling.
        /// </summary>
        void Configure(BackgroundExceptionHandlingOptionsBuilder builder);
    }
}
