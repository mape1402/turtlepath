namespace TurtlePath.Automations.Options
{
    using System.Linq.Expressions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Queries;

    internal sealed class DescriptorGetOneQueryOptions<TQuery, TEntity, TKey, TValue> : IGetOneQueryOptions<TQuery, TEntity>
        where TEntity : class, IEntity<TKey>
    {
        private readonly Func<TQuery, TKey> keySelector;

        public DescriptorGetOneQueryOptions(AutomationDescriptorRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            var descriptor = registry.Descriptors.FirstOrDefault(descriptor =>
                descriptor.OperationKind == AutomationOperationKind.GetOne &&
                descriptor.RequestType == typeof(TQuery) &&
                descriptor.EntityType == typeof(TEntity));

            if (descriptor?.KeySelector != null)
                keySelector = (Func<TQuery, TKey>)descriptor.KeySelector.Compile();
        }

        public Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request)
        {
            var key = keySelector != null
                ? keySelector(request)
                : ResolveKeyFromValue(request);

            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var id = Expression.Property(entity, nameof(IEntity<TKey>.Id));
            var value = Expression.Constant(key, typeof(TKey));
            var equals = Expression.Equal(id, value);

            return Expression.Lambda<Func<TEntity, bool>>(equals, entity);
        }

        private static TKey ResolveKeyFromValue(TQuery request)
        {
            var valueProperty = typeof(TQuery).GetProperty("Value");
            if (valueProperty == null)
                throw new NotSupportedException($"Get-one query '{typeof(TQuery).FullName}' does not expose a Value property.");

            return ConvertValue((TValue)valueProperty.GetValue(request));
        }

        private static TKey ConvertValue(TValue value)
        {
            if (value is TKey key)
                return key;

            throw new NotSupportedException(
                $"Get-one query '{typeof(TQuery).FullName}' requires a key selector because value type '{typeof(TValue).FullName}' cannot be used as key type '{typeof(TKey).FullName}'.");
        }
    }
}
