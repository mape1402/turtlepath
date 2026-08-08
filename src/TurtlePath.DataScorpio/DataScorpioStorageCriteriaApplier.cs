namespace TurtlePath.DataScorpio
{
    using global::DataScorpio.Execution;
    using global::DataScorpio.Parsing;
    using global::DataScorpio.Profiles;
    using global::DataScorpio.Querying;
    using global::DataScorpio.Validation;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Persistence;

    /// <summary>
    /// Applies DataScorpio filters and sorts to TurtlePath storage queries.
    /// </summary>
    public sealed class DataScorpioStorageCriteriaApplier : IStorageCriteriaApplier
    {
        private readonly IQueryParser _parser;
        private readonly IQueryDescriptorValidator _validator;
        private readonly IQueryableQueryApplier _applier;
        private readonly IQueryProfileRegistry _profiles;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataScorpioStorageCriteriaApplier"/> class.
        /// </summary>
        public DataScorpioStorageCriteriaApplier(
            IQueryParser parser,
            IQueryDescriptorValidator validator,
            IQueryableQueryApplier applier,
            IQueryProfileRegistry profiles)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _applier = applier ?? throw new ArgumentNullException(nameof(applier));
            _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
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

            var descriptor = _parser.Parse(new QueryRequest
            {
                Filters = criteria.Filters,
                Sorts = criteria.Sorts
            });
            var profile = _profiles.GetProfile<TEntity>();
            var validation = _validator.Validate(descriptor, profile);

            if (!validation.IsValid)
                throw new DataScorpioTurtlePathQueryException(validation);

            return _applier.Apply(source, descriptor.WithPage(PageDescriptor.Unpaged), profile);
        }
    }
}
