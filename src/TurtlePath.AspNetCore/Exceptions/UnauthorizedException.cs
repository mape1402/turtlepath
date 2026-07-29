namespace TurtlePath.AspNetCore.Exceptions
{
    /// <summary>
    /// Exception thrown when a request is unauthorized.
    /// </summary>
    public sealed class UnauthorizedException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class for the specified user with a default message.
        /// </summary>
        /// <param name="user">The user that is not authorized.</param>
        public UnauthorizedException(string user) : this(user, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedException"/> class for the specified user and message.
        /// </summary>
        /// <param name="user">The user that is not authorized.</param>
        /// <param name="message">Additional message details.</param>
        public UnauthorizedException(string user, string message) : base(System.Net.HttpStatusCode.Unauthorized, $"User '{user}' isn't authorized in this context.{message}")
        {
        }
    }
}
