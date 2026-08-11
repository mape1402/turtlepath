using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore.Conventions;
using TurtlePath.Domain.Identifier;
using TurtlePath.Mapping;
using TurtlePath.Persistence;

namespace TurtlePath.Tests;

public class EntityFrameworkCoreRegistrationTests
{
    [Fact]
    public void UseEntityFrameworkCore_registers_default_options()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();

        services.AddTurtlePath().UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.True(options.ApplyConfigurations);
        Assert.True(options.ApplyBaseEntityConventions);
        Assert.True(options.ApplyCIdConverters);
        Assert.Empty(options.ConfigurationAssemblies);
    }

    [Fact]
    public void UseEntityFrameworkCore_registers_configured_options()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>(options => options with
            {
                ApplyBaseEntityConventions = false,
                ConfigurationAssemblies = [typeof(EntityFrameworkCoreRegistrationTests).Assembly]
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.True(options.ApplyConfigurations);
        Assert.False(options.ApplyBaseEntityConventions);
        Assert.True(options.ApplyCIdConverters);
        Assert.Equal([typeof(EntityFrameworkCoreRegistrationTests).Assembly], options.ConfigurationAssemblies);
    }

    [Fact]
    public void UseEntityFrameworkCore_uses_registered_identifier_definition()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Empty);
                config.DbType = "uniqueidentifier";
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.Parse(value);
                config.JsonConverter = value => CId.Parse(value);
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            })
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.NotNull(options.CIdDefinition);
        Assert.Equal(typeof(string), options.CIdDefinition.DatabaseValueType);
        Assert.Equal("uniqueidentifier", options.CIdDefinition.DatabaseColumnType);
        Assert.True(options.CIdDefinition.HasDatabaseConversion);
    }

    [Fact]
    public void UseEntityFrameworkCore_registers_concrete_context_as_IDbContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();

        Assert.IsType<SampleDbContext>(dbContext);
    }

    [Fact]
    public void UseEntityFrameworkCore_registers_model_conventions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        var conventions = provider.GetServices<ITurtlePathModelConvention>().ToArray();

        Assert.Contains(conventions, convention => convention is BaseEntityModelConvention);
        Assert.Contains(conventions, convention => convention is CIdModelConvention);
    }

    [Fact]
    public void UseEntityFrameworkCore_registers_storage_adapters()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();
        services.AddSingleton<IMapperAdapter, EmptyMapperAdapter>();

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<StorageReaderAdapter>(scope.ServiceProvider.GetRequiredService<IStorageReaderAdapter>());
        Assert.IsType<StorageWriterAdapter>(scope.ServiceProvider.GetRequiredService<IStorageWriterAdapter>());
    }

    [Fact]
    public async Task StorageReaderAdapter_supports_non_async_queryable_after_criteria_applier()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var services = new ServiceCollection();

        services.AddDbContext<SampleDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddSingleton<IMapperAdapter, EmptyMapperAdapter>();
        services.AddSingleton<IStorageCriteriaApplier, InMemoryCriteriaApplier>();

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();

        await using (var seedScope = provider.CreateAsyncScope())
        {
            var context = seedScope.ServiceProvider.GetRequiredService<SampleDbContext>();
            context.Set<SampleEntity>().AddRange(
                new SampleEntity { Id = 1, Name = "Alpha" },
                new SampleEntity { Id = 2, Name = "Beta" });
            await context.SaveChangesAsync();
        }

        await using var scope = provider.CreateAsyncScope();
        var reader = scope.ServiceProvider.GetRequiredService<IStorageReaderAdapter>();

        var batch = await reader.For<SampleEntity>()
            .SortBy("name")
            .Page(1, 10)
            .ToBatchAsync<SampleEntity>();

        Assert.Equal(2, batch.RowCount);
        Assert.Equal(["Alpha", "Beta"], batch.Results.Select(item => item.Name));
    }

    private sealed class SampleDbContext : BaseDbContext
    {
        public SampleDbContext(
            DbContextOptions<SampleDbContext> options,
            TurtlePathDbContextOptions turtlePathOptions,
            IEnumerable<ITurtlePathModelConvention> modelConventions)
            : base(options, turtlePathOptions, modelConventions)
        {
        }

        public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
    }

    private sealed class EmptyMapperAdapter : IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => throw new NotSupportedException();

        public ValueTask UpdateMapAsync<TSource, TDestination>(TSource source, TDestination destination, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => throw new NotSupportedException();
    }

    private sealed class InMemoryCriteriaApplier : IStorageCriteriaApplier
    {
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> source, GetManyCriteria<TEntity> criteria)
            where TEntity : class, TurtlePath.Domain.Contracts.IEntity
            => source.ToArray().AsQueryable();
    }

    private sealed class SampleEntity : TurtlePath.Domain.Contracts.IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
