using System.Net;

namespace TurtlePath.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when request validation fails.
    /// </summary>
    public sealed class ModelValidationException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelValidationException"/> class.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        public ModelValidationException(IEnumerable<string> errors)
            : base(HttpStatusCode.BadRequest, "One or more validation errors occurred.")
        {
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; }
    }
}
