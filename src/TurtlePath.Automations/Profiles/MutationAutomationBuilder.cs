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
                notFoundMessage: notFoundMessage);
    }
}
