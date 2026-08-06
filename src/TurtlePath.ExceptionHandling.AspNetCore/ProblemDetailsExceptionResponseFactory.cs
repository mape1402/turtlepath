using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Builds RFC 7807 problem details responses.
    /// </summary>
    public sealed class ProblemDetailsExceptionResponseFactory : IHttpExceptionResponseFactory
    {
        private readonly ApiBehaviorOptions apiBehaviorOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsExceptionResponseFactory"/> class.
        /// </summary>
        /// <param name="apiBehaviorOptions">The API behavior options.</param>
        public ProblemDetailsExceptionResponseFactory(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            this.apiBehaviorOptions = apiBehaviorOptions?.Value;
        }

        /// <inheritdoc />
        public object Create(ExceptionDescriptor descriptor, int statusCode)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            var problemDetails = new ProblemDetails();

            if (apiBehaviorOptions?.ClientErrorMapping.TryGetValue(statusCode, out var errorData) == true)
            {
                problemDetails.Type = errorData.Link;
                problemDetails.Title = errorData.Title;
            }
            else
            {
                var reason = ReasonPhrases.GetReasonPhrase(statusCode);
                if (!string.IsNullOrWhiteSpace(reason))
                    problemDetails.Title = reason;
            }

            problemDetails.Title ??= "An unexpected error occurred.";
            problemDetails.Detail = string.Join(" | ", descriptor.Messages);
            problemDetails.Status = statusCode;
            problemDetails.Instance = descriptor.TraceIdentifier;
            problemDetails.Extensions["code"] = descriptor.Code;
            problemDetails.Extensions["kind"] = descriptor.Kind.Value;

            foreach (var item in descriptor.Metadata)
                problemDetails.Extensions[item.Key] = item.Value;

            return problemDetails;
        }
    }
}
