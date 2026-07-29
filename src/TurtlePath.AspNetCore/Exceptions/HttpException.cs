using System.Net;

namespace TurtlePath.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents errors that occur during HTTP operations and includes an associated HTTP status code.
    /// </summary>
    public class HttpException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException"/> class with the specified status code and a default message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code associated with the error.</param>
        public HttpException(HttpStatusCode statusCode) : this(statusCode, $"Has occurred an error relationed with HttpStatusCode '{statusCode}'") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpException"/> class with the specified status code and message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code associated with the error.</param>
        /// <param name="message">The message that describes the error.</param>
        public HttpException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP status code associated with the error.
        /// </summary>
        public HttpStatusCode StatusCode { get; }
    }
}
