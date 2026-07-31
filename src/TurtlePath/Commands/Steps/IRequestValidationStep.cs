namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Validates a request before a command operation continues.
    /// </summary>
    public interface IRequestValidationStep<TRequest, TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Validates the request.
        /// </summary>
        ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }
}
