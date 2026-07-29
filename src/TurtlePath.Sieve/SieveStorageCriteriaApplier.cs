namespace TurtlePath.Sieve
{
    using global::Sieve.Models;
    using global::Sieve.Services;
    using TurtlePath.Contracts;
    using TurtlePath.Persistence;

    /// <summary>
    /// Applies Sieve filters and sorts to storage queries.
    /// </summary>
    public sealed class SieveStorageCriteriaApplier : IStorageCriteriaApplier
    {
        private readonly ISieveProcessor _sieveProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SieveStorageCriteriaApplier"/> class.
        /// </summary>
        /// <param name="sieveProcessor">The Sieve processor.</param>
        public SieveStorageCriteriaApplier(ISieveProcessor sieveProcessor)
        {
            _sieveProcessor = sieveProcessor ?? throw new ArgumentNullException(nameof(sieveProcessor));
        }

        /// <inheritdoc/>
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : BaseEntity
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            if (!criteria.UseFilters() && !criteria.UseSorts())
                return source;

            var sieveModel = new SieveModel
            {
                Filters = criteria.Filters,
                Sorts = criteria.Sorts
            };

            return _sieveProcessor.Apply(sieveModel, source, applyPagination: false);
        }
    }
}
