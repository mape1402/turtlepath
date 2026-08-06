using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Maps exception descriptors to HTTP status codes.
    /// </summary>
    public interface IHttpExceptionStatusCodeMapper
    {
        /// <summary>
        /// Maps the descriptor to an HTTP status code.
        /// </summary>
        int Map(ExceptionDescriptor descriptor);
    }
}
