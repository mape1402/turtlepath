namespace TurtlePath.Commands.Steps
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Maps a command entity to its response.
    /// </summary>
    public interface IResponseMappingStep<TRequest, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class
    {
        /// <summary>
        /// Maps the entity to a response.
        /// </summary>
        ValueTask<TResponse> MapAsync(
            TRequest request,
            TEntity entity,
            bool useProjectionFromStorage,
            Expression<Func<TEntity, bool>> projectionFilter,
            Expression<Func<TEntity, object>>[] includeExpressions,
            CancellationToken cancellationToken);
    }
}
