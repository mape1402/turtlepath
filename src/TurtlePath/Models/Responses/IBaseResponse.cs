namespace TurtlePath.Models.Responses
{
    /// <summary>
    /// Defines a response model with an identifier.
    /// </summary>
    /// <typeparam name="TKey">The identifier type.</typeparam>
    public interface IBaseResponse<TKey>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        TKey Id { get; set; }
    }
}
