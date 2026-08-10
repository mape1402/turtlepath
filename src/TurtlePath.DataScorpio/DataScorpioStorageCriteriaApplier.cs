namespace TurtlePath.DataScorpio
{
    using global::DataScorpio.Execution;
    using global::DataScorpio.Querying;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    /// <summary>
    /// Applies DataScorpio filters and sorts to TurtlePath storage queries.
    /// </summary>
    public sealed class DataScorpioStorageCriteriaApplier : IStorageCriteriaApplier
    {
        private readonly IQueryProcessor queryProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataScorpioStorageCriteriaApplier"/> class.
        /// </summary>
        /// <param name="queryProcessor">The DataScorpio query processor.</param>
        public DataScorpioStorageCriteriaApplier(IQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor ?? throw new ArgumentNullException(nameof(queryProcessor));
        }

        /// <inheritdoc/>
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : class, IEntity
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            if (!criteria.UseFilters() && !criteria.UseSorts())
                return source;

            var result = queryProcessor.Execute(source, new QueryRequest
            {
                Filters = criteria.Filters,
                Sorts = criteria.Sorts
            });

            if (!result.IsSuccess)
                throw new DataScorpioQueryException(result.Validation);

            return result.Result.Items.AsQueryable();
        }
    }
}
