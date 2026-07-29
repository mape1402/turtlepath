namespace TurtlePath.Validation
{
    /// <summary>
    /// Exception thrown when request validation fails.
    /// </summary>
    public sealed class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        public ValidationException(IEnumerable<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors?.ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IReadOnlyCollection<string> Errors { get; }
    }
}
