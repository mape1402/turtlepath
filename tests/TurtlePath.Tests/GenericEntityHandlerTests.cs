using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Commands;
using TurtlePath.Domain.Contracts;
using TurtlePath.Hooks;
using TurtlePath.Mapping;
using TurtlePath.Models.Responses;
using TurtlePath.Persistence;
using TurtlePath.Queries;
using TurtlePath.Validation;

namespace TurtlePath.Tests;

public class GenericEntityHandlerTests
{
    [Fact]
    public async Task Create_handler_supports_entities_with_custom_key_contract()
    {
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new CreateCustomEntityHandler(provider);

        var response = await handler.Handle(new CreateCustomEntityRequest("Ada"));

        var entity = Assert.Single(storage.AddedEntities.OfType<CustomEntity>());
        Assert.Equal(10, entity.Id);
        Assert.Equal("Ada", entity.Name);
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.Name, response.Name);
    }

    [Fact]
    public async Task Create_handler_runs_command_hooks_by_stage_order()
    {
        var calls = new List<string>();
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<CreateCommandStageHook>();
            });

        var handler = new CreateCustomEntityHandler(provider);

        await handler.Handle(new CreateCustomEntityRequest("Ada"));

        Assert.Equal(
            [
                "before-validation",
                "after-validation",
                "before-map",
                "after-map",
                "before-save",
                "after-save",
                "before-response",
                "after-response"
            ],
            calls);
    }

    [Fact]
    public async Task Create_no_return_handler_supports_entities_with_custom_key_contract()
    {
        var calls = new List<string>();
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<CreateNoReturnCommandStageHook>();
            });

        var handler = new CreateCustomEntityNoReturnHandler(provider);

        await handler.Handle(new CreateCustomEntityNoReturnRequest("Linus"));

        var entity = Assert.Single(storage.AddedEntities.OfType<CustomEntity>());
        Assert.Equal(11, entity.Id);
        Assert.Equal("Linus", entity.Name);
        Assert.Equal(
            [
                "before-validation",
                "after-validation",
                "before-map",
                "after-map",
                "before-save",
                "after-save"
            ],
            calls);
    }

    [Fact]
    public async Task Get_by_id_handler_supports_entities_with_custom_key_contract()
    {
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity
        {
            Id = 42,
            Name = "Grace"
        });

        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            reader,
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new GetCustomEntityByIdHandler(provider);

        var response = await handler.Handle(new GetCustomEntityByIdQuery(42));

        Assert.Equal(42, response.Id);
        Assert.Equal("Grace", response.Name);
    }

    [Fact]
    public async Task Get_by_id_handler_runs_query_hooks_by_stage_order()
    {
        var calls = new List<string>();
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity
        {
            Id = 42,
            Name = "Grace"
        });

        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            reader,
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<GetByIdQueryStageHook>();
            });

        var handler = new GetCustomEntityByIdHandler(provider);

        await handler.Handle(new GetCustomEntityByIdQuery(42));

        Assert.Equal(["before-query", "after-query"], calls);
    }

    private static ServiceProvider CreateProvider(
        IStorageWriterAdapter storageWriterAdapter,
        IStorageReaderAdapter storageReaderAdapter,
        IMapperAdapter mapperAdapter,
        IValidatorAdapter validatorAdapter,
        Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();

        services.AddTurtlePath();
        services.AddSingleton(storageWriterAdapter);
        services.AddSingleton(storageReaderAdapter);
        services.AddSingleton(mapperAdapter);
        services.AddSingleton(validatorAdapter);
        configure?.Invoke(services);

        return services.BuildServiceProvider();
    }

    private sealed record CreateCustomEntityRequest(string Name) : IRequest<CustomResponse>;

    private sealed record CreateCustomEntityNoReturnRequest(string Name) : IRequest;

    private sealed class GetCustomEntityByIdQuery : GenericGetByIdQuery<CustomEntity, CustomResponse, int>
    {
        public GetCustomEntityByIdQuery(int id) : base(id)
        {
        }
    }

    private sealed class CustomEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class CustomResponse : IBaseResponse<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class CreateCustomEntityHandler
        : GenericCreateCommandHandler<CreateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public CreateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class CreateCustomEntityNoReturnHandler
        : GenericCreateNoReturnCommandHandler<CreateCustomEntityNoReturnRequest, CustomEntity, int>
    {
        public CreateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class GetCustomEntityByIdHandler
        : GenericGetByIdQueryHandler<GetCustomEntityByIdQuery, CustomEntity, CustomResponse, int>
    {
        public GetCustomEntityByIdHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class CreateCommandStageHook(List<string> calls) :
        IBeforeValidationHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterValidationHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeMapHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterMapHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeSaveHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterSaveHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeResponseHook<CreateCustomEntityRequest, CustomEntity, CustomResponse>,
        IAfterResponseHook<CreateCustomEntityRequest, CustomEntity, CustomResponse>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-validation");

        public ValueTask AfterValidationAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-validation");

        public ValueTask BeforeMapAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-map");

        public ValueTask AfterMapAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-map");

        public ValueTask BeforeSaveAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-save");

        public ValueTask AfterSaveAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-save");

        public ValueTask BeforeResponseAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("before-response");

        public ValueTask AfterResponseAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("after-response");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CreateNoReturnCommandStageHook(List<string> calls) :
        IBeforeValidationHook<CreateCustomEntityNoReturnRequest, CustomEntity>,
        IAfterValidationHook<CreateCustomEntityNoReturnRequest, CustomEntity>,
        IBeforeMapHook<CreateCustomEntityNoReturnRequest, CustomEntity>,
        IAfterMapHook<CreateCustomEntityNoReturnRequest, CustomEntity>,
        IBeforeSaveHook<CreateCustomEntityNoReturnRequest, CustomEntity>,
        IAfterSaveHook<CreateCustomEntityNoReturnRequest, CustomEntity>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-validation");

        public ValueTask AfterValidationAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-validation");

        public ValueTask BeforeMapAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-map");

        public ValueTask AfterMapAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-map");

        public ValueTask BeforeSaveAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-save");

        public ValueTask AfterSaveAsync(CommandHookContext<CreateCustomEntityNoReturnRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-save");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class GetByIdQueryStageHook(List<string> calls) :
        IBeforeQueryHook<GetCustomEntityByIdQuery, CustomResponse>,
        IAfterQueryHook<GetCustomEntityByIdQuery, CustomResponse>
    {
        public ValueTask BeforeQueryAsync(QueryHookContext<GetCustomEntityByIdQuery, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("before-query");

        public ValueTask AfterQueryAsync(QueryHookContext<GetCustomEntityByIdQuery, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("after-query");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestMapperAdapter : IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            object result = source switch
            {
                CreateCustomEntityRequest request when typeof(TDestination) == typeof(CustomEntity) => new CustomEntity
                {
                    Id = 10,
                    Name = request.Name
                },
                CreateCustomEntityNoReturnRequest request when typeof(TDestination) == typeof(CustomEntity) => new CustomEntity
                {
                    Id = 11,
                    Name = request.Name
                },
                CustomEntity entity when typeof(TDestination) == typeof(CustomResponse) => new CustomResponse
                {
                    Id = entity.Id,
                    Name = entity.Name
                },
                _ => throw new InvalidOperationException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not configured.")
            };

            return ValueTask.FromResult((TDestination)result);
        }

        public ValueTask UpdateMapAsync<TSource, TDestination>(
            TSource source,
            TDestination destination,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => ValueTask.CompletedTask;
    }

    private sealed class NoopValidatorAdapter : IValidatorAdapter
    {
        public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    private sealed class RecordingStorageWriterAdapter : IStorageWriterAdapter
    {
        public List<object> AddedEntities { get; } = [];

        public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            AddedEntities.Add(entity);
            return ValueTask.CompletedTask;
        }

        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            AddedEntities.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
        }

        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity
        {
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => Task.CompletedTask;

        public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => Task.CompletedTask;

        public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => Task.CompletedTask;
    }

    private sealed class EmptyStorageReaderAdapter : IStorageReaderAdapter
    {
        public IStorageReadSet<TEntity> For<TEntity>() where TEntity : class, IEntity
            => new InMemoryStorageReadSet<TEntity>([]);

        public Task<TExpected> GetOneAsync<TEntity, TExpected>(
            GetOneCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => Task.FromResult<TExpected>(null);

        public Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(
            GetManyCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => Task.FromResult(new BatchResult<TExpected>());
    }

    private sealed class InMemoryStorageReaderAdapter : IStorageReaderAdapter
    {
        private readonly IReadOnlyCollection<object> entities;

        public InMemoryStorageReaderAdapter(params object[] entities)
        {
            this.entities = entities;
        }

        public IStorageReadSet<TEntity> For<TEntity>() where TEntity : class, IEntity
            => new InMemoryStorageReadSet<TEntity>(entities.OfType<TEntity>().ToList());

        public Task<TExpected> GetOneAsync<TEntity, TExpected>(
            GetOneCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => For<TEntity>()
                .Where(criteria.FiltersExpression)
                .FirstOrDefaultAsync<TExpected>(cancellationToken);

        public Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(
            GetManyCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => For<TEntity>().ToBatchAsync<TExpected>(cancellationToken);
    }

    private sealed class InMemoryStorageReadSet<TEntity> : IStorageReadSet<TEntity>
        where TEntity : class, IEntity
    {
        private IEnumerable<TEntity> entities;

        public InMemoryStorageReadSet(IEnumerable<TEntity> entities)
        {
            this.entities = entities;
        }

        public IStorageReadSet<TEntity> Where(Expression<Func<TEntity, bool>> filter)
        {
            if (filter != null)
                entities = entities.Where(filter.Compile()).ToList();

            return this;
        }

        public IStorageReadSet<TEntity> FilterBy(string filters) => this;

        public IStorageReadSet<TEntity> SortBy(Expression<Func<TEntity, object>> sort) => this;

        public IStorageReadSet<TEntity> SortByDescending(Expression<Func<TEntity, object>> sort) => this;

        public IStorageReadSet<TEntity> SortBy(string sorts) => this;

        public IStorageReadSet<TEntity> AsTracking() => this;

        public IStorageReadSet<TEntity> AsNoTracking() => this;

        public IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize) => this;

        public Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var entity = entities.FirstOrDefault();

            return Task.FromResult(Map<TExpected>(entity));
        }

        public Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var results = entities
                .Select(Map<TExpected>)
                .Where(result => result != null)
                .ToArray();

            return Task.FromResult(new BatchResult<TExpected>
            {
                Results = results
            });
        }

        private static TExpected Map<TExpected>(TEntity entity)
            where TExpected : class
        {
            if (entity == null)
                return null;

            if (entity is TExpected expected)
                return expected;

            if (entity is CustomEntity customEntity && typeof(TExpected) == typeof(CustomResponse))
            {
                return new CustomResponse
                {
                    Id = customEntity.Id,
                    Name = customEntity.Name
                } as TExpected;
            }

            throw new InvalidOperationException($"Projection from {typeof(TEntity).Name} to {typeof(TExpected).Name} is not configured.");
        }
    }
}
