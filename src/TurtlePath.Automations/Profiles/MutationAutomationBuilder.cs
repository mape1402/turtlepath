namespace TurtlePath.Automations.Profiles
{
    using System.Linq.Expressions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;

    internal sealed class MutationAutomationBuilder<TRequest, TEntity, TKey> : IMutationAutomationBuilder<TRequest, TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        private Expression<Func<TRequest, TKey>> keySelector;
        private string notFoundMessage;
        private bool? validateRequest;
        private bool? reloadBeforeResponse;
        private Type handlerType;
        private readonly List<Expression<Func<TEntity, object>>> responseIncludeExpressions = [];
        private readonly Dictionary<string, object> metadata = new(StringComparer.Ordinal);

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> GetKeyFrom(Expression<Func<TRequest, TKey>> keySelector)
        {
            this.keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> NotFoundMessage(string message)
        {
            notFoundMessage = message;
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> ValidateRequest(bool validateRequest = true)
        {
            this.validateRequest = validateRequest;
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> Validate(bool validateRequest = true)
            => ValidateRequest(validateRequest);

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> ReloadBeforeResponse(bool reloadBeforeResponse = true)
        {
            this.reloadBeforeResponse = reloadBeforeResponse;
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> Projection(bool enabled = true)
            => ReloadBeforeResponse(enabled);

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> Include(Expression<Func<TEntity, object>> includeExpression)
        {
            if (includeExpression == null)
                throw new ArgumentNullException(nameof(includeExpression));

            reloadBeforeResponse ??= true;
            responseIncludeExpressions.Add(includeExpression);
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> UseHandler<THandler>() where THandler : class
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
            AutomationReturnMode returnMode,
            Type responseType = null)
            => new(
                operationKind,
                requestType,
                entityType,
                keyType,
                returnMode,
                responseType,
                AutomationSourceKind.Profile,
                keySelector,
                notFoundMessage: notFoundMessage,
                validateRequest: validateRequest,
                reloadBeforeResponse: reloadBeforeResponse ?? false,
                responseIncludeExpressions: responseIncludeExpressions,
                handlerType: handlerType,
                metadata: metadata);
    }
}
