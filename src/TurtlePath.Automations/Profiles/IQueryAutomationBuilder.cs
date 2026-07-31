namespace TurtlePath.Automations.Profiles
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Configures query automation behavior for one query/entity pair.
    /// </summary>
    public interface IQueryAutomationBuilder<TQuery, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Configures how the entity key is read from the query.
        /// </summary>
        IQueryAutomationBuilder<TQuery, TEntity, TKey> GetKeyFrom(Expression<Func<TQuery, TKey>> keySelector);

        /// <summary>
        /// Configures the default sort property for paged queries.
        /// </summary>
        IQueryAutomationBuilder<TQuery, TEntity, TKey> DefaultSort(string propertyName);

        /// <summary>
        /// Configures the not-found message for entity lookup queries.
        /// </summary>
        IQueryAutomationBuilder<TQuery, TEntity, TKey> NotFoundMessage(string message);
    }
}
