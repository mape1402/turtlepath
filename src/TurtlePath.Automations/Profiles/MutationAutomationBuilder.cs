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
        private bool reloadBeforeResponse;
        private readonly List<Expression<Func<TEntity, object>>> responseIncludeExpressions = [];

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

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> ReloadBeforeResponse()
        {
            reloadBeforeResponse = true;
            return this;
        }

        public IMutationAutomationBuilder<TRequest, TEntity, TKey> Include(Expression<Func<TEntity, object>> includeExpression)
        {
            if (includeExpression == null)
                throw new ArgumentNullException(nameof(includeExpression));

            reloadBeforeResponse = true;
            responseIncludeExpressions.Add(includeExpression);
            return this;
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
                reloadBeforeResponse: reloadBeforeResponse,
                responseIncludeExpressions: responseIncludeExpressions);
    }
}
