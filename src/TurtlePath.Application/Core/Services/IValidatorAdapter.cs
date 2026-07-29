namespace TurtlePath.Core.Services
{
    /// <summary>
    /// Defines an abstraction for validating request objects.
    /// </summary>
    public interface IValidatorAdapter
    {
        /// <summary>
        /// Asynchronously validates the specified validation request object.
        /// </summary>
        /// <typeparam name="TModel">The type of the model to validate.</typeparam>
        /// <param name="model">The validation request containing the model to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous validation operation.</returns>
        ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default);
    }
}