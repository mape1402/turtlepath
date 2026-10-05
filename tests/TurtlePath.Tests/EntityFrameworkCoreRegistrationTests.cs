using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore.Conventions;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using TurtlePath.Mapping;
using TurtlePath.Models.Responses;
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
    public void UseEntityFrameworkCore_validates_builder_argument()
    {
        ITurtlePathBuilder builder = null;

        Assert.Throws<ArgumentNullException>(() =>
            builder.UseEntityFrameworkCore<SampleDbContext>());
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
    public void UseEntityFrameworkCore_keeps_configured_identifier_definitions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();
        var registry = new CIdDefinitionRegistry();
        registry.Register(CreateCIdDatabaseDefinition());

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>(options => options with
            {
                CIdDefinitions = registry
            });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.Same(registry, options.CIdDefinitions);
        Assert.Null(options.CIdDefinition);
    }

    [Fact]
    public void UseEntityFrameworkCore_handles_missing_default_identifier_definition()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<SampleDbContext>().Options);
        services.AddScoped<SampleDbContext>();
        services.AddSingleton<ICIdDefinitionRegistry>(new CIdDefinitionRegistry());

        services
            .AddTurtlePath()
            .UseEntityFrameworkCore<SampleDbContext>();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.Null(options.CIdDefinition);
        Assert.NotNull(options.CIdDefinitions);
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

    [Fact]
    public async Task StorageReaderAdapter_validates_get_one_criteria()
    {
        await using var context = CreateInMemoryContext();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            reader.GetOneAsync<SampleEntity, SampleEntity>(null));

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            reader.GetOneAsync<SampleEntity, SampleEntity>(new GetOneCriteria<SampleEntity>()));

        Assert.Equal("criteria", exception.ParamName);
    }

    [Fact]
    public async Task StorageReaderAdapter_maps_single_entity_when_expected_type_differs()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.Add(new SampleEntity { Id = 3, Name = "Gamma" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var response = await reader.GetOneAsync<SampleEntity, SampleResponse>(
            new GetOneCriteria<SampleEntity>
            {
                FiltersExpression = entity => entity.Id == 3,
                UseTracking = false
            });

        Assert.Equal(3, response.Id);
        Assert.Equal("Gamma", response.Name);
    }

    [Fact]
    public async Task StorageReaderAdapter_returns_entity_directly_and_null_when_missing()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.Add(new SampleEntity { Id = 4, Name = "Delta" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var entity = await reader.GetOneAsync<SampleEntity, SampleEntity>(
            new GetOneCriteria<SampleEntity>
            {
                FiltersExpression = item => item.Id == 4,
                UseTracking = true
            });
        var missing = await reader.GetOneAsync<SampleEntity, SampleResponse>(
            new GetOneCriteria<SampleEntity>
            {
                FiltersExpression = item => item.Id == 404,
                UseTracking = false
            });

        Assert.Same(context.SampleEntities.Local.Single(item => item.Id == 4), entity);
        Assert.Null(missing);
    }

    [Fact]
    public async Task StorageReaderAdapter_applies_filter_sort_paging_and_maps_results()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.AddRange(
            new SampleEntity { Id = 1, Name = "Charlie" },
            new SampleEntity { Id = 2, Name = "Alpha" },
            new SampleEntity { Id = 3, Name = "Bravo" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var batch = await reader.GetManyAsync<SampleEntity, SampleResponse>(
            new GetManyCriteria<SampleEntity>
            {
                FiltersExpression = entity => entity.Id >= 1,
                SortingExpression = entity => entity.Name,
                AscendentSort = true,
                PageNumber = 1,
                PageSize = 2,
                UseTracking = false
            });

        Assert.Equal(3, batch.RowCount);
        Assert.Equal(2, batch.PageCount);
        Assert.Equal(1, batch.PageNumber);
        Assert.Equal(2, batch.PageSize);
        Assert.Equal(["Alpha", "Bravo"], batch.Results.Select(item => item.Name));
    }

    [Fact]
    public async Task StorageReaderAdapter_handles_empty_paged_result()
    {
        await using var context = CreateInMemoryContext();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var batch = await reader.GetManyAsync<SampleEntity, SampleEntity>(
            new GetManyCriteria<SampleEntity>
            {
                PageNumber = 1,
                PageSize = 10
            });

        Assert.Equal(0, batch.RowCount);
        Assert.Equal(0, batch.PageCount);
        Assert.Equal(0, batch.PageNumber);
        Assert.Equal(10, batch.PageSize);
        Assert.Empty(batch.Results);
    }

    [Fact]
    public async Task StorageReaderAdapter_handles_unpaged_results_and_includes()
    {
        await using var context = CreateInMemoryContext();
        context.ParentEntities.AddRange(
            new ParentEntity
            {
                Id = 1,
                Name = "Parent B",
                Child = new ChildEntity { Id = 10, Name = "Child B" }
            },
            new ParentEntity
            {
                Id = 2,
                Name = "Parent A",
                Child = new ChildEntity { Id = 11, Name = "Child A" }
            });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var batch = await reader.GetManyAsync<ParentEntity, ParentEntity>(
            new GetManyCriteria<ParentEntity>
            {
                IncludeExpressions = [entity => entity.Child],
                SortingExpression = entity => entity.Name,
                AscendentSort = false,
                UseTracking = true
            });

        Assert.Equal(2, batch.RowCount);
        Assert.Equal(1, batch.PageCount);
        Assert.Equal(1, batch.PageNumber);
        Assert.Equal(2, batch.PageSize);
        Assert.Equal(["Parent B", "Parent A"], batch.Results.Select(entity => entity.Name));
        Assert.All(batch.Results, entity => Assert.NotNull(entity.Child));
    }

    [Fact]
    public async Task StorageReadSet_accumulates_fluent_options()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.AddRange(
            new SampleEntity { Id = 1, Name = "Charlie" },
            new SampleEntity { Id = 2, Name = "Alpha" },
            new SampleEntity { Id = 3, Name = "Bravo" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var batch = await reader
            .For<SampleEntity>()
            .AsTracking()
            .AsNoTracking()
            .Where(entity => entity.Id > 1)
            .SortByDescending(entity => entity.Name)
            .Page(1, 1)
            .ToBatchAsync<SampleEntity>();

        Assert.Equal(2, batch.RowCount);
        Assert.Equal(2, batch.PageCount);
        Assert.Equal("Bravo", Assert.Single(batch.Results).Name);
    }

    [Fact]
    public async Task StorageReadSet_applies_ascending_expression_sort()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.AddRange(
            new SampleEntity { Id = 1, Name = "Charlie" },
            new SampleEntity { Id = 2, Name = "Alpha" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter());

        var batch = await reader
            .For<SampleEntity>()
            .SortBy(entity => entity.Name)
            .ToBatchAsync<SampleEntity>();

        Assert.Equal(["Alpha", "Charlie"], batch.Results.Select(entity => entity.Name));
    }

    [Fact]
    public async Task StorageReadSet_first_or_default_uses_accumulated_options()
    {
        await using var context = CreateInMemoryContext();
        context.ParentEntities.Add(new ParentEntity
        {
            Id = 7,
            Name = "Included",
            Child = new ChildEntity { Id = 17, Name = "Included child" }
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter(), [new InMemoryCriteriaApplier()]);

        var entity = await reader
            .For<ParentEntity>()
            .AsTracking()
            .Include(null, parent => parent.Child)
            .FilterBy("Name==Included")
            .Where(parent => parent.Id == 7)
            .FirstOrDefaultAsync<ParentEntity>();

        Assert.NotNull(entity);
        Assert.NotNull(entity.Child);
        Assert.True(context.Entry(entity).State != EntityState.Detached);
    }

    [Fact]
    public async Task StorageReadSet_accepts_empty_include_and_string_sort()
    {
        await using var context = CreateInMemoryContext();
        context.SampleEntities.Add(new SampleEntity { Id = 9, Name = "Nine" });
        await context.SaveChangesAsync();
        var reader = new StorageReaderAdapter(context, new SampleMapperAdapter(), [new InMemoryCriteriaApplier()]);

        var batch = await reader
            .For<SampleEntity>()
            .Include()
            .Include(null)
            .FilterBy("Name==Nine")
            .SortBy("Name")
            .Page(null, null)
            .ToBatchAsync<SampleEntity>();

        Assert.Equal("Nine", Assert.Single(batch.Results).Name);
    }

    [Fact]
    public async Task StorageWriterAdapter_persists_update_and_delete_operations()
    {
        await using var context = CreateInMemoryContext();
        var writer = new StorageWriterAdapter(context);
        var entity = new SampleEntity { Id = 5, Name = "Original" };

        await writer.SaveAsync(entity);
        entity.Name = "Updated";
        await writer.UpdateAsync(entity);

        Assert.Equal("Updated", await context.SampleEntities.Where(item => item.Id == 5).Select(item => item.Name).SingleAsync());

        await writer.DeleteAsync(entity);

        Assert.Empty(context.SampleEntities);
    }

    [Fact]
    public async Task StorageWriterAdapter_supports_direct_batch_operations()
    {
        await using var context = CreateInMemoryContext();
        var writer = new StorageWriterAdapter(context);
        var first = new SampleEntity { Id = 20, Name = "Twenty" };
        var second = new SampleEntity { Id = 21, Name = "Twenty one" };

        await writer.AddRangeAsync([first, second]);
        await writer.SaveChangesAsync();

        first.Name = "Updated";
        second.Name = "Also updated";
        writer.Update(first);
        writer.UpdateRange([second]);
        await writer.SaveChangesAsync();

        Assert.Equal(["Updated", "Also updated"], await context.SampleEntities.OrderBy(entity => entity.Id).Select(entity => entity.Name).ToArrayAsync());

        writer.RemoveRange([first, second]);
        await writer.SaveChangesAsync();

        Assert.Empty(context.SampleEntities);
    }

    [Fact]
    public void Storage_adapters_validate_constructor_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => new StorageReaderAdapter(null, new SampleMapperAdapter()));
        Assert.Throws<ArgumentNullException>(() => new StorageReaderAdapter(CreateInMemoryContext(), null));
        Assert.Throws<ArgumentNullException>(() => new StorageWriterAdapter(null));
    }

    [Fact]
    public void Model_conventions_apply_base_entity_key_and_cid_converter()
    {
        var builder = new ModelBuilder(new ConventionSet());
        var turtlePathOptions = TurtlePathDbContextOptions.Default with
        {
            CIdDefinition = CreateCIdDatabaseDefinition()
        };

        builder.Entity<CIdEntity>().Property(entity => entity.Id);
        builder.Entity<CIdEntity>().Property(entity => entity.Name);
        new BaseEntityModelConvention().Apply(builder, turtlePathOptions);
        new CIdModelConvention().Apply(builder, turtlePathOptions);

        var entityType = builder.Model.FindEntityType(typeof(CIdEntity));
        var idProperty = entityType.FindProperty(nameof(CIdEntity.Id));

        Assert.Equal([nameof(CIdEntity.Id)], entityType.FindPrimaryKey().Properties.Select(property => property.Name));
        Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
        Assert.NotNull(idProperty.GetValueConverter());
        Assert.Equal("varchar(64)", idProperty.FindAnnotation("Relational:ColumnType")?.Value);
    }

    [Fact]
    public void Model_conventions_skip_when_options_disable_them_or_converter_is_missing()
    {
        var disabledBuilder = new ModelBuilder(new ConventionSet());
        var missingConverterBuilder = new ModelBuilder(new ConventionSet());
        var disabledOptions = TurtlePathDbContextOptions.Default with
            {
                ApplyBaseEntityConventions = false,
                ApplyCIdConverters = false,
                CIdDefinition = CreateCIdDatabaseDefinition()
            };
        var missingConverterOptions = TurtlePathDbContextOptions.Default with
            {
                CIdDefinition = new CIdDefinition(
                    CIdDefinition.DefaultContext,
                    null,
                    CIdDefinition.DefaultPropertyName,
                    typeof(string),
                    () => CId.From("empty"),
                    CId.Parse,
                    id => id.ToString(),
                    id => id.ToByteArray(),
                    CIdGenerationStrategy.ClientGenerated)
            };

        disabledBuilder.Entity<CIdEntity>().Property(entity => entity.Id);
        disabledBuilder.Entity<CIdEntity>().Property(entity => entity.Name);
        missingConverterBuilder.Entity<CIdEntity>().Property(entity => entity.Id);
        missingConverterBuilder.Entity<CIdEntity>().Property(entity => entity.Name);
        new BaseEntityModelConvention().Apply(disabledBuilder, disabledOptions);
        new CIdModelConvention().Apply(disabledBuilder, disabledOptions);
        new CIdModelConvention().Apply(missingConverterBuilder, missingConverterOptions);

        var disabledProperty = disabledBuilder.Model.FindEntityType(typeof(CIdEntity)).FindProperty(nameof(CIdEntity.Id));
        var missingConverterProperty = missingConverterBuilder.Model.FindEntityType(typeof(CIdEntity)).FindProperty(nameof(CIdEntity.Id));

        Assert.NotEqual(ValueGenerated.OnAdd, disabledProperty.ValueGenerated);
        Assert.Null(disabledProperty.GetValueConverter());
        Assert.Null(missingConverterProperty.GetValueConverter());
    }

    private static SampleDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SampleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new SampleDbContext(
            options,
            TurtlePathDbContextOptions.Default,
            []);
    }

    private static CIdDefinition CreateCIdDatabaseDefinition()
        => new(
            CIdDefinition.DefaultContext,
            null,
            CIdDefinition.DefaultPropertyName,
            typeof(string),
            () => CId.From("empty"),
            CId.Parse,
            id => id.ToString(),
            id => id.ToByteArray(),
            CIdGenerationStrategy.ClientGenerated,
            typeof(string),
            "varchar(64)",
            (System.Linq.Expressions.Expression<Func<CId, string>>)(id => id.ToString()),
            (System.Linq.Expressions.Expression<Func<string, CId>>)(value => CId.Parse(value)));

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

        public DbSet<ParentEntity> ParentEntities => Set<ParentEntity>();

        public DbSet<ChildEntity> ChildEntities => Set<ChildEntity>();
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

    private sealed class CIdConventionDbContext : BaseDbContext
    {
        public CIdConventionDbContext(
            DbContextOptions<CIdConventionDbContext> options,
            TurtlePathDbContextOptions turtlePathOptions,
            IEnumerable<ITurtlePathModelConvention> modelConventions)
            : base(options, turtlePathOptions, modelConventions)
        {
        }

        public DbSet<CIdEntity> CIdEntities => Set<CIdEntity>();
    }

    private sealed class SampleMapperAdapter : IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(TSource source, CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            if (source is SampleEntity entity && typeof(TDestination) == typeof(SampleResponse))
            {
                object response = new SampleResponse
                {
                    Id = entity.Id,
                    Name = entity.Name
                };

                return ValueTask.FromResult((TDestination)response);
            }

            throw new NotSupportedException();
        }

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

    private sealed class SampleEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class ParentEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ChildId { get; set; }

        public ChildEntity Child { get; set; }
    }

    private sealed class ChildEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class SampleResponse : IBaseResponse<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class CIdEntity : BaseEntity
    {
        public string Name { get; set; }
    }
}
