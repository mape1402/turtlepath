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
        private Type handlerType;
        private readonly Dictionary<string, object> metadata = new(StringComparer.Ordinal);

        public QueryAutomationBuilder(IReadOnlyDictionary<string, object> metadataDefaults = null)
        {
            if (metadataDefaults is null)
                return;

            foreach (var item in metadataDefaults)
                metadata[item.Key] = item.Value;
        }

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

        public IQueryAutomationBuilder<TQuery, TEntity, TKey> UseHandler<THandler>() where THandler : class
        {
            handlerType = typeof(THandler);
            return this;
        }

        public void SetMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            metadata[key] = value ?? throw new ArgumentNullException(nameof(value));
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
                notFoundMessage,
                handlerType: handlerType,
                metadata: metadata);
    }
}
