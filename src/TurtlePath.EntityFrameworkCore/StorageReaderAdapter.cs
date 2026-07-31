namespace TurtlePath.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;
    using TurtlePath.Domain.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides an implementation of <see cref="IStorageReaderAdapter"/> for reading entities from storage with support for filtering, sorting, and paging.
    /// </summary>
    public class StorageReaderAdapter : IStorageReaderAdapter
    {
        private readonly IDbContext _dbContext;
        private readonly IMapperAdapter _mapper;
        private readonly IEnumerable<IStorageCriteriaApplier> _criteriaAppliers;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageReaderAdapter"/> class.
        /// </summary>
        /// <param name="dbContext">The database context for entity access.</param>
        /// <param name="mapper">The mapper adapter for result projection.</param>
        /// <param name="criteriaAppliers">The provider-specific criteria appliers.</param>
        public StorageReaderAdapter(IDbContext dbContext, IMapperAdapter mapper, IEnumerable<IStorageCriteriaApplier> criteriaAppliers = null)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _criteriaAppliers = criteriaAppliers ?? Enumerable.Empty<IStorageCriteriaApplier>();
        }

        /// <inheritdoc/>
        public IStorageReadSet<TEntity> For<TEntity>() where TEntity : class, IEntity
            => new StorageReadSet<TEntity>(this);

        /// <summary>
        /// Asynchronously retrieves a single entity matching the specified criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TExpected">The type of the expected result.</typeparam>
        /// <param name="criteria">The criteria for selecting the entity.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the expected result as the result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if criteria is null.</exception>
        /// <exception cref="ArgumentException">Thrown if FiltersExpression is null.</exception>
        public async Task<TExpected> GetOneAsync<TEntity, TExpected>(GetOneCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            if (criteria.FiltersExpression == null)
                throw new ArgumentException("FiltersExpression cannot be null.", nameof(criteria));

            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (!criteria.UseTracking)
                query = query.AsNoTracking();

            query = query.Where(criteria.FiltersExpression);

            if(typeof(TEntity) == typeof(TExpected))
            {
                var entity = await query.FirstOrDefaultAsync(cancellationToken);
                return entity as TExpected;
            }

            var source = await query.FirstOrDefaultAsync(cancellationToken);

            return source == null
                ? null
                : await _mapper.MapAsync<TEntity, TExpected>(source, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves multiple entities matching the specified criteria, with support for filtering, sorting, and paging.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TExpected">The type of the expected result.</typeparam>
        /// <param name="criteria">The criteria for selecting the entities.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with a batch result containing the expected results as the result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if criteria is null.</exception>
        public async Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(GetManyCriteria<TEntity> criteria, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
        {
            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (!criteria.UseTracking)
                query = query.AsNoTracking();

            query = ApplyExpressions(query, criteria);
            query = ApplyProviderCriteria(query, criteria);
            var pagedQuery = ApplyPaging(query, criteria);

            var results = Enumerable.Empty<TExpected>();

            if (typeof(TEntity) == typeof(TExpected))
            {
                var entities = await pagedQuery.query.ToListAsync(cancellationToken);
                results = entities.Cast<TExpected>();
            }
            else
            {
                var entities = await pagedQuery.query.ToListAsync(cancellationToken);
                var mapped = new List<TExpected>(entities.Count);

                foreach (var entity in entities)
                    mapped.Add(await _mapper.MapAsync<TEntity, TExpected>(entity, cancellationToken));

                results = mapped;
            }

            return new BatchResult<TExpected>
            {
                PageSize = pagedQuery.pageSize,
                PageNumber = pagedQuery.pageNumber,
                RowCount = pagedQuery.rowCount,
                PageCount = pagedQuery.pageCount,
                Results = results
            };
        }

        /// <summary>
        /// Applies filter and sorting expressions to the queryable source.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The queryable source.</param>
        /// <param name="criteria">The criteria containing expressions.</param>
        /// <returns>The queryable source with expressions applied.</returns>
        private IQueryable<TEntity> ApplyExpressions<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : class, IEntity
        {
            if (criteria.UseFiltersExpression())
                source = source.Where(criteria.FiltersExpression);

            if (criteria.UseSortingExpression())
            {
                source = criteria.AscendentSort
                    ? source.OrderBy(criteria.SortingExpression)
                    : source.OrderByDescending(criteria.SortingExpression);
            }

            return source;
        }

        /// <summary>
        /// Applies provider-specific filtering and sorting to the queryable source.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The queryable source.</param>
        /// <param name="criteria">The criteria containing Sieve filters and sorts.</param>
        /// <returns>The queryable source with provider-specific filters and sorts applied.</returns>
        private IQueryable<TEntity> ApplyProviderCriteria<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : class, IEntity
        {
            foreach (var criteriaApplier in _criteriaAppliers)
                source = criteriaApplier.Apply(source, criteria);

            return source;
        }

        /// <summary>
        /// Applies paging to the queryable source based on the criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The queryable source.</param>
        /// <param name="criteria">The criteria containing paging information.</param>
        /// <returns>A tuple containing the paged query, row count, page count, page number, and page size.</returns>
        private (IQueryable<TEntity> query, int rowCount, int pageCount, int pageNumber, int pageSize) ApplyPaging<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : class, IEntity
        {
            var rowCount = source.Count();

            if (!criteria.UsePaging())
                return (source, rowCount, 1, 1, rowCount);

            if(rowCount == default)
                return (source, rowCount, 0, 0, criteria.PageSize.Value);

            var pageCount = rowCount / criteria.PageSize.Value + (rowCount % criteria.PageSize.Value > 0 ? 1 : 0);

            var pageSize = criteria.PageSize.Value;
            var pageNumber = criteria.PageNumber.Value > pageCount ? pageCount : criteria.PageNumber.Value;

            return (source.Skip((pageNumber - 1) * pageSize).Take(pageSize), rowCount, pageCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Stores fluent read options before delegating execution to the storage reader adapter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        private sealed class StorageReadSet<TEntity> : IStorageReadSet<TEntity> where TEntity : class, IEntity
        {
            /// <summary>
            /// The reader used to execute the accumulated criteria.
            /// </summary>
            private readonly StorageReaderAdapter _reader;

            /// <summary>
            /// The criteria accumulated by the fluent read operations.
            /// </summary>
            private readonly GetManyCriteria<TEntity> _criteria = new() { UseTracking = false };

            /// <summary>
            /// Initializes a new instance of the <see cref="StorageReadSet{TEntity}"/> class.
            /// </summary>
            /// <param name="reader">The reader used to execute the accumulated criteria.</param>
            public StorageReadSet(StorageReaderAdapter reader)
            {
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> Where(Expression<Func<TEntity, bool>> filter)
            {
                _criteria.FiltersExpression = filter;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> FilterBy(string filters)
            {
                _criteria.Filters = filters;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> SortBy(Expression<Func<TEntity, object>> sort)
            {
                _criteria.SortingExpression = sort;
                _criteria.AscendentSort = true;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> SortByDescending(Expression<Func<TEntity, object>> sort)
            {
                _criteria.SortingExpression = sort;
                _criteria.AscendentSort = false;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> SortBy(string sorts)
            {
                _criteria.Sorts = sorts;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> AsTracking()
            {
                _criteria.UseTracking = true;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> AsNoTracking()
            {
                _criteria.UseTracking = false;
                return this;
            }

            /// <inheritdoc/>
            public IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize)
            {
                _criteria.PageNumber = pageNumber;
                _criteria.PageSize = pageSize;
                return this;
            }

            /// <inheritdoc/>
            public Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
                where TExpected : class
            {
                return _reader.GetOneAsync<TEntity, TExpected>(new GetOneCriteria<TEntity>
                {
                    FiltersExpression = _criteria.FiltersExpression,
                    Filters = _criteria.Filters,
                    UseTracking = _criteria.UseTracking
                }, cancellationToken);
            }

            /// <inheritdoc/>
            public Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
                where TExpected : class
                => _reader.GetManyAsync<TEntity, TExpected>(_criteria, cancellationToken);
        }
    }
}
