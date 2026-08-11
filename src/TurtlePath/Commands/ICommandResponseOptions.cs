namespace TurtlePath.Commands
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Configures how a command handler builds its response after a mutation.
    /// </summary>
    /// <typeparam name="TRequest">The request type handled by the command.</typeparam>
    /// <typeparam name="TEntity">The mutated entity type.</typeparam>
    public interface ICommandResponseOptions<TRequest, TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Gets a value indicating whether the response should be projected from storage after the mutation is saved.
        /// </summary>
        bool UseProjectionFromStorage { get; }

        /// <summary>
        /// Gets navigation expressions to include when the response is projected from storage.
        /// </summary>
        /// <param name="request">The command request.</param>
        /// <returns>The navigation expressions to include.</returns>
        Expression<Func<TEntity, object>>[] GetIncludeExpressions(TRequest request);
    }
}
