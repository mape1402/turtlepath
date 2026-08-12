namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Defines ASP.NET Core exception handling mappings.
    /// </summary>
    public interface IHttpExceptionHandlingProfile
    {
        /// <summary>
        /// Configures ASP.NET Core exception handling.
        /// </summary>
        void Configure(HttpExceptionHandlingOptionsBuilder builder);
    }
}
