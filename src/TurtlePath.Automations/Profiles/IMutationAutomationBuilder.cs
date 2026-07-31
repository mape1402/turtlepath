namespace TurtlePath.Automations.Profiles
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;

    /// <summary>
    /// Configures mutation automation behavior for one request/entity pair.
    /// </summary>
    public interface IMutationAutomationBuilder<TRequest, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Configures how the entity key is read from the request.
        /// </summary>
        IMutationAutomationBuilder<TRequest, TEntity, TKey> GetKeyFrom(Expression<Func<TRequest, TKey>> keySelector);

        /// <summary>
        /// Configures the not-found message for entity lookup operations.
        /// </summary>
        IMutationAutomationBuilder<TRequest, TEntity, TKey> NotFoundMessage(string message);
    }
}
