namespace TurtlePath.DataScorpio
{
    using global::DataScorpio.Validation;

    /// <summary>
    /// Exception thrown when a TurtlePath criteria query is rejected by DataScorpio strict validation.
    /// </summary>
    public sealed class DataScorpioTurtlePathQueryException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataScorpioTurtlePathQueryException"/> class.
        /// </summary>
        /// <param name="validation">The validation result.</param>
        public DataScorpioTurtlePathQueryException(QueryValidationResult validation)
            : base(BuildMessage(validation))
        {
            Validation = validation ?? throw new ArgumentNullException(nameof(validation));
        }

        /// <summary>
        /// Gets the failed validation result.
        /// </summary>
        public QueryValidationResult Validation { get; }

        private static string BuildMessage(QueryValidationResult validation)
        {
            if (validation == null || validation.Errors.Count == 0)
                return "The DataScorpio TurtlePath query was rejected.";

            return "The DataScorpio TurtlePath query was rejected: " +
                string.Join("; ", validation.Errors.Select(error => error.Message));
        }
    }
}
