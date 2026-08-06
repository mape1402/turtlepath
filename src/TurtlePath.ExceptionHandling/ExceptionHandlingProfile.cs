namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Base class for transport-neutral exception handling mapping profiles.
    /// </summary>
    public abstract class ExceptionHandlingProfile : IExceptionHandlingProfile
    {
        /// <inheritdoc />
        public abstract void Configure(ExceptionHandlingOptionsBuilder builder);
    }
}
