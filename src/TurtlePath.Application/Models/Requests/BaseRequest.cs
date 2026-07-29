namespace TurtlePath.Core.Models.Requests
{
    using TurtlePath.Identifier;

    /// <summary>
    /// Represents the base request model containing a unique identifier.
    /// </summary>
    public abstract class BaseRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        public CId Id { get; set; }
    }
}
