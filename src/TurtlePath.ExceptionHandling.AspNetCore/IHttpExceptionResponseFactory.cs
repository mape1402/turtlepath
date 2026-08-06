using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Builds HTTP response objects for exception descriptors.
    /// </summary>
    public interface IHttpExceptionResponseFactory
    {
        /// <summary>
        /// Creates the HTTP response body.
        /// </summary>
        object Create(ExceptionDescriptor descriptor, int statusCode);
    }
}
