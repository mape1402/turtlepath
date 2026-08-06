using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Provides HTTP exception projection options.
    /// </summary>
    public sealed class HttpExceptionHandlingOptions
    {
        /// <summary>
        /// Gets HTTP status code mappings by exception kind.
        /// </summary>
        public IDictionary<ExceptionKind, int> StatusCodeMappings { get; } = new Dictionary<ExceptionKind, int>
        {
            [ExceptionKind.Validation] = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
            [ExceptionKind.Business] = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
            [ExceptionKind.NotFound] = Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound,
            [ExceptionKind.Conflict] = Microsoft.AspNetCore.Http.StatusCodes.Status409Conflict,
            [ExceptionKind.Unauthorized] = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized,
            [ExceptionKind.Forbidden] = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden,
            [ExceptionKind.Transient] = Microsoft.AspNetCore.Http.StatusCodes.Status503ServiceUnavailable,
            [ExceptionKind.Failure] = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError
        };
    }
}
