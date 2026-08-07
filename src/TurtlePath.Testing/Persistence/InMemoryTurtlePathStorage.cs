namespace TurtlePath.Testing.Persistence
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;

    /// <summary>
    /// In-memory TurtlePath storage adapter for unit and lightweight integration tests.
    /// </summary>
    public sealed class InMemoryTurtlePathStorage : IStorageReaderAdapter, IStorageWriterAdapter
    {
        private readonly Dictionary<Type, List<object>> entities = [];
        private readonly IMapperAdapter mapper;

        /// <summary>
        /// Initializes a new instance of the storage.
        /// </summary>
        public InMemoryTurtlePathStorage(IMapperAdapter mapper)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets the recorded storage operations.
        /// </summary>
        public List<StorageOperation> Operations { get; } = [];

        /// <summary>
        /// Gets a typed entity set.
        /// </summary>
        public InMemoryEntitySet<TEntity> Set<TEntity>()
            where TEntity : class, IEntity
            => new(this);

        /// <summary>
        /// Seeds entities into the store.
        /// </summary>
        public InMemoryTurtlePathStorage Seed<TEntity>(params TEntity[] seedEntities)
            where TEntity : class, IEntity
        {
            if (seedEntities == null)
                throw new ArgumentNullException(nameof(seedEntities));

            var set = GetOrCreateSet(typeof(TEntity));
            set.AddRange(seedEntities);

            return this;
        }

        /// <summary>
        /// Gets all entities for the specified type.
        /// </summary>
        public IReadOnlyList<TEntity> Entities<TEntity>()
            where TEntity : class, IEntity
            => GetOrCreateSet(typeof(TEntity)).OfType<TEntity>().ToArray();

        /// <summary>
        /// Clears all stored entities and operations.
        /// </summary>
        public void Clear()
        {
            entities.Clear();
            Operations.Clear();
        }

        /// <inheritdoc />
        public IStorageReadSet<TEntity> For<TEntity>()
            where TEntity : class, IEntity
            => new InMemoryStorageReadSet<TEntity>(Entities<TEntity>(), mapper);

        /// <inheritdoc />
        public Task<TExpected> GetOneAsync<TEntity, TExpected>(
            GetOneCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
        {
            var readSet = For<TEntity>();

            if (criteria?.FiltersExpression != null)
                readSet.Where(criteria.FiltersExpression);

            return readSet.FirstOrDefaultAsync<TExpected>(cancellationToken);
        }

        /// <inheritdoc />
        public Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(
            GetManyCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
        {
            var readSet = For<TEntity>();

            if (criteria != null)
            {
                if (criteria.FiltersExpression != null)
                    readSet.Where(criteria.FiltersExpression);

                if (criteria.SortingExpression != null)
                {
                    if (criteria.AscendentSort)
                        readSet.SortBy(criteria.SortingExpression);
                    else
                        readSet.SortByDescending(criteria.SortingExpression);
                }

                if (criteria.UsePaging())
                    readSet.Page(criteria.PageNumber, criteria.PageSize);
            }

            return readSet.ToBatchAsync<TExpected>(cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            GetOrCreateSet(typeof(TEntity)).Add(entity);
            Operations.Add(new StorageOperation("Add", typeof(TEntity), entity));

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            var set = GetOrCreateSet(typeof(TEntity));

            foreach (var entity in items)
            {
                set.Add(entity);
                Operations.Add(new StorageOperation("Add", typeof(TEntity), entity));
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Update<TEntity>(TEntity entity)
            where TEntity : class, IEntity
            => Operations.Add(new StorageOperation("Update", typeof(TEntity), entity));

        /// <inheritdoc />
        public void UpdateRange<TEntity>(IEnumerable<TEntity> items)
            where TEntity : class, IEntity
        {
            foreach (var entity in items)
                Update(entity);
        }

        /// <inheritdoc />
        public void Remove<TEntity>(TEntity entity)
            where TEntity : class, IEntity
        {
            GetOrCreateSet(typeof(TEntity)).Remove(entity);
            Operations.Add(new StorageOperation("Remove", typeof(TEntity), entity));
        }

        /// <inheritdoc />
        public void RemoveRange<TEntity>(IEnumerable<TEntity> items)
            where TEntity : class, IEntity
        {
            foreach (var entity in items.ToArray())
                Remove(entity);
        }

        /// <inheritdoc />
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Operations.Add(new StorageOperation("SaveChanges", typeof(IEntity), null));
            return Task.FromResult(1);
        }

        /// <inheritdoc />
        [Obsolete("Use AddAsync followed by SaveChangesAsync.")]
        public async Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            await AddAsync(entity, cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        [Obsolete("Use Update when the entity is detached, then SaveChangesAsync.")]
        public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            Update(entity);
            return SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        [Obsolete("Use Remove followed by SaveChangesAsync.")]
        public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            Remove(entity);
            return SaveChangesAsync(cancellationToken);
        }

        private List<object> GetOrCreateSet(Type entityType)
        {
            if (!entities.TryGetValue(entityType, out var set))
            {
                set = [];
                entities[entityType] = set;
            }

            return set;
        }
    }
}
