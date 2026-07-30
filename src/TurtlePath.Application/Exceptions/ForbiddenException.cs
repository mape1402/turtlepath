using System.Net;

namespace TurtlePath.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts to access a resource they are not allowed to access.
    /// </summary>
    public sealed class ForbiddenException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class for a specific resource and user.
        /// </summary>
        /// <param name="resource">The resource the user attempted to access.</param>
        /// <param name="user">The user who is not allowed to access the resource.</param>
        public ForbiddenException(string resource, string user) : this($"User '{user}' is not allowed to access the resource '{resource}'")
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ForbiddenException(string message) : base(HttpStatusCode.Forbidden, message)
        {
        }
    }
}
