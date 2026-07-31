namespace TurtlePath.Queries
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides query-specific filtering for generic get-one handlers.
    /// </summary>
    public interface IGetOneQueryOptions<TQuery, TEntity>
    {
        /// <summary>
        /// Builds the filter expression used to find the target entity.
        /// </summary>
        Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request);
    }
}
