using Microsoft.AspNetCore.Http;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Maps exception kinds to HTTP status codes.
    /// </summary>
    public sealed class DefaultHttpExceptionStatusCodeMapper : IHttpExceptionStatusCodeMapper
    {
        private readonly HttpExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHttpExceptionStatusCodeMapper"/> class.
        /// </summary>
        /// <param name="options">The HTTP exception handling options.</param>
        public DefaultHttpExceptionStatusCodeMapper(Microsoft.Extensions.Options.IOptions<HttpExceptionHandlingOptions> options)
        {
            this.options = options?.Value ?? new HttpExceptionHandlingOptions();
        }

        /// <inheritdoc />
        public int Map(ExceptionDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            return options.StatusCodeMappings.TryGetValue(descriptor.Kind, out var statusCode)
                ? statusCode
                : StatusCodes.Status500InternalServerError;
        }
    }
}
