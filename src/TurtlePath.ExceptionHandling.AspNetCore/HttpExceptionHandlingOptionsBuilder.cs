using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Provides fluent configuration for HTTP exception handling options.
    /// </summary>
    public sealed class HttpExceptionHandlingOptionsBuilder
    {
        private readonly HttpExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpExceptionHandlingOptionsBuilder"/> class.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        public HttpExceptionHandlingOptionsBuilder(HttpExceptionHandlingOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Maps an exception kind to an HTTP status code.
        /// </summary>
        public HttpExceptionHandlingOptionsBuilder Map(ExceptionKind kind, int statusCode)
        {
            if (kind == null)
                throw new ArgumentNullException(nameof(kind));

            options.StatusCodeMappings[kind] = statusCode;

            return this;
        }
    }
}
