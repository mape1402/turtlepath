namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Base class for ASP.NET Core exception handling profiles.
    /// </summary>
    public abstract class HttpExceptionHandlingProfile : IHttpExceptionHandlingProfile
    {
        /// <inheritdoc />
        public abstract void Configure(HttpExceptionHandlingOptionsBuilder builder);
    }
}
