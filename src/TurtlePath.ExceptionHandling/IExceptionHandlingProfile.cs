namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Configures transport-neutral exception handling mappings.
    /// </summary>
    public interface IExceptionHandlingProfile
    {
        /// <summary>
        /// Configures exception mappings.
        /// </summary>
        void Configure(ExceptionHandlingOptionsBuilder builder);
    }
}
