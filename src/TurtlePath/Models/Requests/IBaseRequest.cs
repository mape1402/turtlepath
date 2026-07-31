namespace TurtlePath.Models.Requests
{
    /// <summary>
    /// Defines a request model with an identifier.
    /// </summary>
    /// <typeparam name="TKey">The identifier type.</typeparam>
    public interface IBaseRequest<TKey>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the resource.
        /// </summary>
        TKey Id { get; set; }
    }
}
