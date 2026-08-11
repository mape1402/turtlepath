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

        /// <summary>
        /// Configures the handler to build the response by reading the entity again from storage.
        /// </summary>
        IMutationAutomationBuilder<TRequest, TEntity, TKey> ReloadBeforeResponse();

        /// <summary>
        /// Includes a navigation when the response is read again from storage.
        /// </summary>
        /// <param name="includeExpression">The navigation expression to include.</param>
        IMutationAutomationBuilder<TRequest, TEntity, TKey> Include(Expression<Func<TEntity, object>> includeExpression);
    }
}
