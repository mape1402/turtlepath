namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Represents a semantic exception kind independent from a transport target.
    /// </summary>
    /// <param name="Value">The kind value.</param>
    public sealed record ExceptionKind(string Value)
    {
        /// <summary>
        /// Validation or input error.
        /// </summary>
        public static readonly ExceptionKind Validation = new("validation");

        /// <summary>
        /// Business rule error.
        /// </summary>
        public static readonly ExceptionKind Business = new("business");

        /// <summary>
        /// Missing resource error.
        /// </summary>
        public static readonly ExceptionKind NotFound = new("not_found");

        /// <summary>
        /// Resource conflict error.
        /// </summary>
        public static readonly ExceptionKind Conflict = new("conflict");

        /// <summary>
        /// Unauthorized error.
        /// </summary>
        public static readonly ExceptionKind Unauthorized = new("unauthorized");

        /// <summary>
        /// Forbidden error.
        /// </summary>
        public static readonly ExceptionKind Forbidden = new("forbidden");

        /// <summary>
        /// Transient failure.
        /// </summary>
        public static readonly ExceptionKind Transient = new("transient");

        /// <summary>
        /// Unexpected failure.
        /// </summary>
        public static readonly ExceptionKind Failure = new("failure");

        /// <inheritdoc />
        public override string ToString() => Value;
    }
}
