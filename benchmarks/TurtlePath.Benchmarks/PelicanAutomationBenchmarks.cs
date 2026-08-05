using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Automations;
using TurtlePath.Automations.Attributes;
using TurtlePath.Commands;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using TurtlePath.Mapping;
using TurtlePath.Models.Responses;
using TurtlePath.Persistence;
using TurtlePath.Validation;

namespace TurtlePath.Benchmarks;

[MemoryDiagnoser]
public class PelicanAutomationBenchmarks
{
    private ServiceProvider _manualProvider;
    private ServiceProvider _automationProvider;
    private IMediator _manualMediator;
    private IMediator _automationMediator;
    private ManualCreateCommand _manualRequest;
    private AutomatedCreateCommand _automationRequest;

    [GlobalSetup]
    public void Setup()
    {
        _manualRequest = new ManualCreateCommand(CId.From(Guid.Parse("2c4cad13-6d27-49db-b9ba-9632ecf00b11")), "manual");
        _automationRequest = new AutomatedCreateCommand(CId.From(Guid.Parse("6a835589-c5b9-4644-a052-568a653731f1")), "automated");

        _manualProvider = CreateServices(useAutomations: false).BuildServiceProvider();
        _automationProvider = CreateServices(useAutomations: true).BuildServiceProvider();
        _manualMediator = _manualProvider.GetRequiredService<IMediator>();
        _automationMediator = _automationProvider.GetRequiredService<IMediator>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _manualProvider.Dispose();
        _automationProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public Task<BenchmarkResponse> PelicanManualHandler()
        => _manualMediator.Send(_manualRequest);

    [Benchmark]
    public Task<BenchmarkResponse> TurtlePathAutomatedHandler()
        => _automationMediator.Send(_automationRequest);

    private static ServiceCollection CreateServices(bool useAutomations)
    {
        var services = new ServiceCollection();
        var assembly = typeof(PelicanAutomationBenchmarks).Assembly;

        services.AddPelican(assembly);
        services.AddSingleton<IMapperAdapter, BenchmarkMapperAdapter>();
        services.AddSingleton<IValidatorAdapter, NoOpValidatorAdapter>();
        services.AddSingleton<IStorageWriterAdapter, NoOpStorageWriterAdapter>();
        services.AddSingleton<IStorageReaderAdapter, NoOpStorageReaderAdapter>();

        var builder = services.AddTurtlePath(assembly);
        if (useAutomations)
            builder.UseAutomations(assembly);

        return services;
    }
}

public sealed record ManualCreateCommand(CId Id, string Name) : IRequest<BenchmarkResponse>;

[CreateAutomation(typeof(BenchmarkEntity), typeof(BenchmarkResponse))]
public sealed record AutomatedCreateCommand(CId Id, string Name) : IRequest<BenchmarkResponse>;

public sealed class ManualCreateCommandHandler
    : GenericCreateCommandHandler<ManualCreateCommand, BenchmarkResponse, BenchmarkEntity, CId>
{
    public ManualCreateCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}

public sealed class BenchmarkEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public sealed class BenchmarkResponse : BaseResponse
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class BenchmarkMapperAdapter : IMapperAdapter
{
    public ValueTask<TDestination> MapAsync<TSource, TDestination>(
        TSource source,
        CancellationToken cancellationToken = default)
        where TSource : class
        where TDestination : class
    {
        object result = source switch
        {
            ManualCreateCommand request when typeof(TDestination) == typeof(BenchmarkEntity) => new BenchmarkEntity
            {
                Id = request.Id,
                Name = request.Name
            },
            AutomatedCreateCommand request when typeof(TDestination) == typeof(BenchmarkEntity) => new BenchmarkEntity
            {
                Id = request.Id,
                Name = request.Name
            },
            BenchmarkEntity entity when typeof(TDestination) == typeof(BenchmarkResponse) => new BenchmarkResponse
            {
                Id = entity.Id,
                Name = entity.Name
            },
            _ => throw new InvalidOperationException($"No benchmark mapping exists from {typeof(TSource).Name} to {typeof(TDestination).Name}.")
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

internal sealed class NoOpValidatorAdapter : IValidatorAdapter
{
    public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

internal sealed class NoOpStorageWriterAdapter : IStorageWriterAdapter
{
    public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
        => ValueTask.CompletedTask;

    public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
        => Task.CompletedTask;

    public void Update<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
    }

    public void UpdateRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity
    {
    }

    public void Remove<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
    }

    public void RemoveRange<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class, IEntity
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

internal sealed class NoOpStorageReaderAdapter : IStorageReaderAdapter
{
    public IStorageReadSet<TEntity> For<TEntity>()
        where TEntity : class, IEntity
        => new NoOpStorageReadSet<TEntity>();

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
        => Task.FromResult(new BatchResult<TExpected>
        {
            Results = []
        });
}

internal sealed class NoOpStorageReadSet<TEntity> : IStorageReadSet<TEntity>
    where TEntity : class, IEntity
{
    public IStorageReadSet<TEntity> Where(System.Linq.Expressions.Expression<Func<TEntity, bool>> filter) => this;

    public IStorageReadSet<TEntity> FilterBy(string filters) => this;

    public IStorageReadSet<TEntity> SortBy(System.Linq.Expressions.Expression<Func<TEntity, object>> sort) => this;

    public IStorageReadSet<TEntity> SortByDescending(System.Linq.Expressions.Expression<Func<TEntity, object>> sort) => this;

    public IStorageReadSet<TEntity> SortBy(string sorts) => this;

    public IStorageReadSet<TEntity> AsTracking() => this;

    public IStorageReadSet<TEntity> AsNoTracking() => this;

    public IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize) => this;

    public Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
        where TExpected : class
        => Task.FromResult<TExpected>(null);

    public Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
        where TExpected : class
        => Task.FromResult(new BatchResult<TExpected>
        {
            Results = []
        });
}
