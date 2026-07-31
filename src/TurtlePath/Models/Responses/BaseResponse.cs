namespace TurtlePath.Models.Responses
{
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Represents the base response model containing a unique identifier.
    /// </summary>
    public abstract class BaseResponse : IBaseResponse<CId>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        public CId Id { get; set; }
    }
}


