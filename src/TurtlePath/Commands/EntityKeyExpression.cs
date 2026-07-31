namespace TurtlePath.Commands
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;

    internal static class EntityKeyExpression
    {
        public static Expression<Func<TEntity, bool>> Equals<TEntity, TKey>(TKey id)
            where TEntity : class, IEntity<TKey>
        {
            var entity = Expression.Parameter(typeof(TEntity), "entity");
            var entityId = Expression.Property(entity, nameof(IEntity<TKey>.Id));
            var value = Expression.Constant(id, typeof(TKey));
            var equals = Expression.Equal(entityId, value);

            return Expression.Lambda<Func<TEntity, bool>>(equals, entity);
        }
    }
}
