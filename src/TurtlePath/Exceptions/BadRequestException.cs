using System.Net;

namespace TurtlePath.Exceptions
{
    /// <summary>
    /// Exception thrown when a bad request is made.
    /// </summary>
    public sealed class BadRequestException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class with a default message.
        /// </summary>
        public BadRequestException() : base(HttpStatusCode.BadRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadRequestException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BadRequestException(string message) : base(HttpStatusCode.BadRequest, message)
        {
        }
    }
}

