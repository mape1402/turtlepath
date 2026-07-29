namespace TurtlePath.Application.Models.Responses
{
    using TurtlePath.Identifier;

    /// <summary>
    /// Represents the base response model containing a unique identifier.
    /// </summary>
    public abstract class BaseResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        public CId Id { get; set; }
    }
}
