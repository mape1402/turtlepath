namespace TurtlePath.Testing.Persistence
{
    using System.Linq.Expressions;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;

    internal sealed class InMemoryStorageReadSet<TEntity> : IStorageReadSet<TEntity>
        where TEntity : class, IEntity
    {
        private readonly IMapperAdapter mapper;
        private IEnumerable<TEntity> query;
        private int? pageNumber;
        private int? pageSize;

        public InMemoryStorageReadSet(IEnumerable<TEntity> entities, IMapperAdapter mapper)
        {
            query = entities ?? [];
            this.mapper = mapper;
        }

        public IStorageReadSet<TEntity> Where(Expression<Func<TEntity, bool>> filter)
        {
            if (filter != null)
                query = query.Where(filter.Compile()).ToArray();

            return this;
        }

        public IStorageReadSet<TEntity> FilterBy(string filters) => this;

        public IStorageReadSet<TEntity> SortBy(Expression<Func<TEntity, object>> sort)
        {
            if (sort != null)
                query = query.OrderBy(sort.Compile()).ToArray();

            return this;
        }

        public IStorageReadSet<TEntity> SortByDescending(Expression<Func<TEntity, object>> sort)
        {
            if (sort != null)
                query = query.OrderByDescending(sort.Compile()).ToArray();

            return this;
        }

        public IStorageReadSet<TEntity> SortBy(string sorts) => this;

        public IStorageReadSet<TEntity> AsTracking() => this;

        public IStorageReadSet<TEntity> AsNoTracking() => this;

        public IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize)
        {
            this.pageNumber = pageNumber;
            this.pageSize = pageSize;

            return this;
        }

        public async Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var entity = query.FirstOrDefault();

            return entity == null
                ? null
                : await MapAsync<TExpected>(entity, cancellationToken);
        }

        public async Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var all = query.ToArray();
            var results = all;

            if (pageNumber.HasValue && pageSize.HasValue && pageNumber.Value > 0 && pageSize.Value > 0)
                results = all.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value).ToArray();

            var mapped = new List<TExpected>();

            foreach (var entity in results)
                mapped.Add(await MapAsync<TExpected>(entity, cancellationToken));

            var actualPageSize = pageSize ?? mapped.Count;
            var actualPageNumber = pageNumber ?? 1;

            return new BatchResult<TExpected>
            {
                PageNumber = actualPageNumber,
                PageSize = actualPageSize,
                RowCount = all.Length,
                PageCount = actualPageSize <= 0 ? 0 : (int)Math.Ceiling(all.Length / (double)actualPageSize),
                Results = mapped
            };
        }

        private ValueTask<TExpected> MapAsync<TExpected>(TEntity entity, CancellationToken cancellationToken)
            where TExpected : class
        {
            if (entity is TExpected expected)
                return ValueTask.FromResult(expected);

            return mapper.MapAsync<TEntity, TExpected>(entity, cancellationToken);
        }
    }
}
