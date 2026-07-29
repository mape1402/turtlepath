namespace TurtlePath.Core.Services
{
    using TurtlePath.Contracts;

    /// <summary>
    /// Applies provider-specific criteria to an entity query.
    /// </summary>
    public interface IStorageCriteriaApplier
    {
        /// <summary>
        /// Applies criteria to the source query.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="criteria">The requested criteria.</param>
        /// <returns>The query with provider criteria applied.</returns>
        IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : BaseEntity;
    }
}
