namespace TurtlePath.Automations.Options
{
    using System.Linq.Expressions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;

    internal sealed class DescriptorCommandResponseOptions<TRequest, TEntity, TKey> : ICommandResponseOptions<TRequest, TEntity>
        where TEntity : class, IEntity<TKey>
    {
        private readonly AutomationDescriptor descriptor;

        public DescriptorCommandResponseOptions(AutomationDescriptor descriptor)
        {
            this.descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public bool UseProjectionFromStorage => descriptor.ReloadBeforeResponse;

        public Expression<Func<TEntity, object>>[] GetIncludeExpressions(TRequest request)
            => descriptor.ResponseIncludeExpressions
                .Cast<Expression<Func<TEntity, object>>>()
                .ToArray();
    }
}
