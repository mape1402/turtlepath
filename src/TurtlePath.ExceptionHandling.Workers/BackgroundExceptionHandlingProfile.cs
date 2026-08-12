namespace TurtlePath.ExceptionHandling.Workers
{
    /// <summary>
    /// Base class for background worker exception handling profiles.
    /// </summary>
    public abstract class BackgroundExceptionHandlingProfile : IBackgroundExceptionHandlingProfile
    {
        /// <inheritdoc />
        public abstract void Configure(BackgroundExceptionHandlingOptionsBuilder builder);
    }
}
