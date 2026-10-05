using TurtlePath.Domain.Identifier;
using TurtlePath.Domain.Identifier.Json;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using TurtlePath.Commands.Steps;
using TurtlePath.Domain.Contracts;
using TurtlePath.Mapping;

namespace TurtlePath.Tests;

public class CIdTests
{
    [Fact]
    public void CId_does_not_expose_a_default_generator()
    {
        Assert.Null(typeof(CId).GetMethod("New", Type.EmptyTypes));
        Assert.Empty(typeof(CId).GetConstructors());
    }

    [Fact]
    public void Factory_new_uses_configured_factory()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("f8cb21f2-35d7-419b-9f58-90d1c82154f0"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.From(Guid.Parse(value));
                config.JsonConverter = value => CId.From(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            });

        using var provider = services.BuildServiceProvider();
        var id = provider.GetRequiredService<ICIdFactory>().New();
        var definition = provider.GetRequiredService<ICIdDefinitionRegistry>().Get();

        Assert.Equal("f8cb21f2-35d7-419b-9f58-90d1c82154f0", id.ToString());
        Assert.Equal(id.Cast<Guid>().ToByteArray(), definition.ToByteArray(id));
    }

    [Fact]
    public void Constructor_accepts_different_underlying_types()
    {
        Assert.Equal(42, CId.From(42).Cast<int>());
        Assert.Equal(Guid.Empty, CId.From(Guid.Empty).Cast<Guid>());
    }

    [Fact]
    public void Parse_detects_supported_identifier_values()
    {
        var guid = Guid.Parse("7fbcefe4-bf3c-42d7-9f13-7877d08d59e4");

        Assert.True(CId.Parse(null).IsEmpty);
        Assert.True(CId.Parse("   ").IsEmpty);
        Assert.Equal(guid, CId.Parse(guid.ToString()).Cast<Guid>());
        Assert.Equal(123, CId.Parse("123").Cast<int>());
        Assert.Equal(2147483648L, CId.Parse("2147483648").Cast<long>());
        Assert.Equal("customer-a", CId.Parse("customer-a").Cast<string>());
    }

    [Fact]
    public void TryParse_returns_parsed_identifier()
    {
        var parsed = CId.TryParse("456", out var id);

        Assert.True(parsed);
        Assert.Equal(456, id.Cast<int>());
    }

    [Fact]
    public void ToByteArray_uses_underlying_value_representation()
    {
        var guid = Guid.Parse("97d9a289-5863-4b8a-8f67-72dc02e4da82");

        Assert.Null(CId.Empty.ToByteArray());
        Assert.Equal(guid.ToByteArray(), CId.From(guid).ToByteArray());
        Assert.Equal(BitConverter.GetBytes(42), CId.From(42).ToByteArray());
        Assert.Equal(BitConverter.GetBytes(2147483648L), CId.From(2147483648L).ToByteArray());
        Assert.Equal(Encoding.UTF8.GetBytes("abc"), CId.From("abc").ToByteArray());
        Assert.Equal(Encoding.UTF8.GetBytes("1.5"), CId.From(1.5m).ToByteArray());
    }

    [Fact]
    public void Equality_uses_underlying_value()
    {
        var left = CId.From(25);
        var right = CId.From(25);
        var different = CId.From(26);

        Assert.True(CId.Empty == default);
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.False(left.Equals("25"));
        Assert.NotEqual(left, different);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
        Assert.Equal(0, CId.Empty.GetHashCode());
        Assert.Equal(string.Empty, CId.Empty.ToString());
    }

    [Fact]
    public void Cast_rejects_empty_or_incompatible_value()
    {
        Assert.Throws<InvalidCastException>(() => CId.Empty.Cast<int>());
        Assert.Throws<InvalidCastException>(() => CId.From("abc").Cast<int>());
        Assert.Throws<ArgumentNullException>(() => CId.From(null));
    }

    [Fact]
    public void Type_converter_converts_from_string_values()
    {
        var converter = TypeDescriptor.GetConverter(typeof(CId));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        var id = Assert.IsType<CId>(converter.ConvertFrom("789"));
        Assert.Equal(789, id.Cast<int>());
    }

    [Fact]
    public void Json_converters_round_trip_required_and_nullable_values()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CIdJsonConverter());
        options.Converters.Add(new CIdNullableJsonConverter());

        var requiredJson = JsonSerializer.Serialize(new RequiredIdPayload { Id = CId.From(99) }, options);
        var required = JsonSerializer.Deserialize<RequiredIdPayload>(requiredJson, options);
        var nullableJson = JsonSerializer.Serialize(new NullableIdPayload { Id = null }, options);
        var nullable = JsonSerializer.Deserialize<NullableIdPayload>("{\"Id\":\"\"}", options);

        Assert.Equal("{\"Id\":\"99\"}", requiredJson);
        Assert.Equal(99, required.Id.Cast<int>());
        Assert.Equal("{\"Id\":null}", nullableJson);
        Assert.Null(nullable.Id);
    }

    [Fact]
    public void Json_options_extension_registers_cid_converters()
    {
        var options = new JsonSerializerOptions();

        var returned = options.AddTurtlePathCIdConverters();

        Assert.Same(options, returned);
        Assert.Contains(options.Converters, converter => converter is CIdJsonConverter);
        Assert.Contains(options.Converters, converter => converter is CIdNullableJsonConverter);
    }

    [Fact]
    public void Nullable_json_converter_writes_non_null_values()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new CIdNullableJsonConverter());

        var json = JsonSerializer.Serialize(new NullableIdPayload { Id = CId.From("nullable") }, options);

        Assert.Equal("{\"Id\":\"nullable\"}", json);
    }

    [Fact]
    public void Configuration_validation_reports_each_required_delegate()
    {
        var config = CreateValidConfiguration();

        config.ValidateAndThrow();
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.ConvertToDb = null; item.ValidateAndThrow(); });
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.ConvertFromDb = null; item.ValidateAndThrow(); });
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.JsonConverter = null; item.ValidateAndThrow(); });
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.NullableJsonConverter = null; item.ValidateAndThrow(); });
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.DefaultFactory = null; item.ValidateAndThrow(); });
        Assert.Throws<InvalidOperationException>(() => { var item = CreateValidConfiguration(); item.ParseFunction = null; item.ValidateAndThrow(); });
    }

    [Fact]
    public void Definition_and_registry_validate_arguments_and_lookup_specific_entities()
    {
        var definition = new CIdDefinition(
            "Customer",
            typeof(CustomerEntity),
            "CustomerId",
            typeof(int),
            () => CId.From(123),
            value => CId.From(int.Parse(value)),
            id => id.ToString(),
            id => BitConverter.GetBytes(id.Cast<int>()),
            CIdGenerationStrategy.StoreGenerated);
        var registry = new CIdDefinitionRegistry();

        registry.Register(definition);

        Assert.False(definition.IsDefault);
        Assert.Equal("Customer", definition.Context);
        Assert.Equal(typeof(CustomerEntity), definition.EntityType);
        Assert.Equal("CustomerId", definition.PropertyName);
        Assert.Equal(typeof(int), definition.ValueType);
        Assert.Equal(CIdGenerationStrategy.StoreGenerated, definition.GenerationStrategy);
        Assert.Equal("123", definition.Formatter(definition.Factory()));
        Assert.Equal(123, definition.Parser("123").Cast<int>());
        Assert.Equal(BitConverter.GetBytes(123), definition.ToByteArray(CId.From(123)));
        Assert.Same(definition, registry.Get(typeof(CustomerEntity), "CustomerId"));
        Assert.True(registry.TryGet(typeof(CustomerEntity), "CustomerId", out var found));
        Assert.Same(definition, found);
        Assert.Throws<ArgumentNullException>(() => registry.Register(null));
        Assert.Throws<InvalidOperationException>(() => new CIdDefinitionRegistry().Get("missing"));
        Assert.Throws<InvalidOperationException>(() => new CIdDefinitionRegistry().Get(typeof(CustomerEntity)));
    }

    [Fact]
    public void Registry_allows_multiple_identifier_definitions()
    {
        var registry = new CIdDefinitionRegistry();
        registry.Register(new CIdDefinition(
            "Customer",
            typeof(CustomerEntity),
            CIdDefinition.DefaultPropertyName,
            typeof(int),
            () => CId.From(0),
            value => CId.From(int.Parse(value)),
            id => id.ToString(),
            id => BitConverter.GetBytes(id.Cast<int>()),
            CIdGenerationStrategy.StoreGenerated));
        registry.Register(new CIdDefinition(
            "Order",
            typeof(OrderEntity),
            CIdDefinition.DefaultPropertyName,
            typeof(Guid),
            () => CId.From(Guid.Parse("e76768cb-ece0-4985-901e-c4c0e434b3fb")),
            value => CId.From(Guid.Parse(value)),
            id => id.ToString(),
            id => id.Cast<Guid>().ToByteArray(),
            CIdGenerationStrategy.ClientGenerated));

        Assert.Equal(0, registry.New("Customer").Cast<int>());
        Assert.Equal("e76768cb-ece0-4985-901e-c4c0e434b3fb", registry.New("Order").ToString());
        Assert.Equal(typeof(int), registry.Get(typeof(CustomerEntity)).ValueType);
        Assert.Equal(typeof(Guid), registry.Get(typeof(OrderEntity)).ValueType);
    }

    [Fact]
    public void Registration_supports_default_identifier_and_entity_overrides()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.From(Guid.Parse(value));
                config.JsonConverter = value => CId.From(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            })
            .UseCIdProfile<LegacyIdentifierProfile>();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ICIdDefinitionRegistry>();

        Assert.Equal(typeof(Guid), registry.Get(typeof(CustomerEntity)).ValueType);
        Assert.Equal(typeof(int), registry.Get(typeof(LegacyEntity)).ValueType);
    }

    [Fact]
    public void Profile_builder_validates_setup_delegate()
    {
        var builder = new CIdProfileBuilder(new CIdDefinitionRegistry());

        Assert.Throws<ArgumentNullException>(() => builder.UseCId<int, int>(null));
        Assert.Throws<ArgumentNullException>(() => builder.UseCIdFor<CustomerEntity, int, int>(null));
    }

    [Fact]
    public void Registration_discovers_identifier_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.From(Guid.Parse(value));
                config.JsonConverter = value => CId.From(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            })
            .UseCIdProfiles(typeof(CIdTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ICIdDefinitionRegistry>();

        Assert.Equal(typeof(int), registry.Get(typeof(LegacyEntity)).ValueType);
    }

    [Fact]
    public async Task Entity_creation_step_assigns_client_generated_identifier()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.From(Guid.Parse(value));
                config.JsonConverter = value => CId.From(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            });
        services.AddSingleton<IMapperAdapter, CIdEntityMapperAdapter>();

        using var provider = services.BuildServiceProvider();
        var step = provider.GetRequiredService<IEntityCreationStep<CreateCIdEntityRequest, CIdCreatedEntity>>();

        var entity = await step.CreateAsync(new CreateCIdEntityRequest("Generated"), CancellationToken.None);

        Assert.Equal("22222222-2222-2222-2222-222222222222", entity.Id.ToString());
        Assert.Equal("Generated", entity.Name);
    }

    [Fact]
    public async Task Entity_creation_step_leaves_identifier_empty_without_client_generation()
    {
        var noRegistryServices = new ServiceCollection();
        noRegistryServices.AddTurtlePath();
        noRegistryServices.AddSingleton<IMapperAdapter, CIdEntityMapperAdapter>();

        await using var noRegistryProvider = noRegistryServices.BuildServiceProvider();
        var noRegistryStep = noRegistryProvider.GetRequiredService<IEntityCreationStep<CreateCIdEntityRequest, CIdCreatedEntity>>();
        var noRegistryEntity = await noRegistryStep.CreateAsync(new CreateCIdEntityRequest("No registry"), CancellationToken.None);

        var storeGeneratedServices = new ServiceCollection();
        storeGeneratedServices
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("33333333-3333-3333-3333-333333333333"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.From(Guid.Parse(value));
                config.JsonConverter = value => CId.From(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(Guid.Parse(value));
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
                config.GenerationStrategy = CIdGenerationStrategy.StoreGenerated;
            });
        storeGeneratedServices.AddSingleton<IMapperAdapter, CIdEntityMapperAdapter>();

        await using var storeGeneratedProvider = storeGeneratedServices.BuildServiceProvider();
        var storeGeneratedStep = storeGeneratedProvider.GetRequiredService<IEntityCreationStep<CreateCIdEntityRequest, CIdCreatedEntity>>();
        var storeGeneratedEntity = await storeGeneratedStep.CreateAsync(new CreateCIdEntityRequest("Store generated"), CancellationToken.None);

        Assert.True(noRegistryEntity.Id.IsEmpty);
        Assert.True(storeGeneratedEntity.Id.IsEmpty);
    }

    private sealed class CustomerEntity
    {
    }

    private sealed class OrderEntity
    {
    }

    private sealed class LegacyEntity
    {
    }

    private sealed record CreateCIdEntityRequest(string Name);

    private sealed class CIdCreatedEntity : IEntity<CId>
    {
        public CId Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class RequiredIdPayload
    {
        public CId Id { get; set; }
    }

    private sealed class NullableIdPayload
    {
        public CId? Id { get; set; }
    }

    private static CIdConfiguration<int, int> CreateValidConfiguration()
        => new()
        {
            DefaultFactory = () => CId.From(1),
            ConvertToDb = id => id.Cast<int>(),
            ConvertFromDb = value => CId.From(value),
            JsonConverter = value => CId.From(int.Parse(value)),
            NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value)),
            ParseFunction = value => CId.From(int.Parse(value)),
            ToByteArrayFunction = value => BitConverter.GetBytes(value)
        };

    private sealed class CIdEntityMapperAdapter : IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            if (source is CreateCIdEntityRequest request && typeof(TDestination) == typeof(CIdCreatedEntity))
            {
                return ValueTask.FromResult(new CIdCreatedEntity { Name = request.Name } as TDestination);
            }

            throw new NotSupportedException();
        }

        public ValueTask UpdateMapAsync<TSource, TDestination>(
            TSource source,
            TDestination destination,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
            => throw new NotSupportedException();
    }

    private sealed class LegacyIdentifierProfile : CIdProfile
    {
        public override void Configure(CIdProfileBuilder builder)
        {
            builder.UseCIdFor<LegacyEntity, int, int>(config =>
            {
                config.DefaultFactory = () => CId.From(0);
                config.ConvertToDb = id => id.Cast<int>();
                config.ConvertFromDb = value => CId.From(value);
                config.JsonConverter = value => CId.From(int.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value));
                config.ParseFunction = value => CId.From(int.Parse(value));
                config.ToByteArrayFunction = value => BitConverter.GetBytes(value);
            });
        }
    }
}

