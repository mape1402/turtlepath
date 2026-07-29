namespace TurtlePath.Application.Errors
{
    /// <summary>
    /// Exception thrown when a requested resource is not found.
    /// </summary>
    public sealed class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class for a specific resource and key.
        /// </summary>
        /// <param name="resource">The resource that was not found.</param>
        /// <param name="key">The key of the resource that was not found.</param>
        public NotFoundException(string resource, string key) : this($"Resource '{resource}' with key '{key}' not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
