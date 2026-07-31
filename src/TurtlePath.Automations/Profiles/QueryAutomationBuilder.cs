namespace TurtlePath.Automations.Profiles
{
    using System.Linq.Expressions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;

    internal sealed class QueryAutomationBuilder<TQuery, TEntity, TKey> : IQueryAutomationBuilder<TQuery, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private Expression<Func<TQuery, TKey>> keySelector;
        private string defaultSortProperty;
        private string notFoundMessage;

        public IQueryAutomationBuilder<TQuery, TEntity, TKey> GetKeyFrom(Expression<Func<TQuery, TKey>> keySelector)
        {
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            return this;
        }

        public IQueryAutomationBuilder<TQuery, TEntity, TKey> DefaultSort(string propertyName)
        {
            defaultSortProperty = propertyName;
            return this;
        }

        public IQueryAutomationBuilder<TQuery, TEntity, TKey> NotFoundMessage(string message)
        {
            notFoundMessage = message;
            return this;
        }

        public AutomationDescriptor CreateDescriptor(
            AutomationOperationKind operationKind,
            Type requestType,
            Type entityType,
            Type keyType,
            Type responseType)
            => new(
                operationKind,
                requestType,
                entityType,
                keyType,
                AutomationReturnMode.Response,
                responseType,
                AutomationSourceKind.Profile,
                keySelector,
                defaultSortProperty,
                notFoundMessage);
    }
}
