using TurtlePath.Domain.Identifier;
using Microsoft.Extensions.DependencyInjection;

namespace TurtlePath.Tests;

public class CIdTests
{
    [Fact]
    public void New_uses_configured_factory()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePath()
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => new CId(Guid.Parse("f8cb21f2-35d7-419b-9f58-90d1c82154f0"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => new CId(Guid.Parse(value));
                config.JsonConverter = value => new CId(Guid.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : new CId(Guid.Parse(value));
                config.ParseFunction = value => new CId(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            });

        using var provider = services.BuildServiceProvider();
        var id = provider.GetRequiredService<ICIdFactory>().New();

        Assert.Equal("f8cb21f2-35d7-419b-9f58-90d1c82154f0", id.ToString());
    }

    [Fact]
    public void Constructor_accepts_different_underlying_types()
    {
        Assert.Equal(42, CId.From(42).Cast<int>());
        Assert.Equal(Guid.Empty, CId.From(Guid.Empty).Cast<Guid>());
    }

    [Fact]
    public void Composite_ids_compare_by_part_name_and_value()
    {
        var left = CId.Composite(
            new CIdPart("TenantId", Guid.Parse("2e80d91a-9025-45b7-a9a5-7d06e7360f82")),
            new CIdPart("OrderNumber", 42));
        var right = CId.Composite(
            new CIdPart("TenantId", Guid.Parse("2e80d91a-9025-45b7-a9a5-7d06e7360f82")),
            new CIdPart("OrderNumber", 42));

        Assert.True(left.IsComposite);
        Assert.Equal(left, right);
        Assert.Equal("TenantId=2e80d91a-9025-45b7-a9a5-7d06e7360f82;OrderNumber=42", left.ToString());
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
            .UseCIdFor<LegacyEntity, int, int>(config =>
            {
                config.DefaultFactory = () => CId.From(0);
                config.ConvertToDb = id => id.Cast<int>();
                config.ConvertFromDb = value => CId.From(value);
                config.JsonConverter = value => CId.From(int.Parse(value));
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value));
                config.ParseFunction = value => CId.From(int.Parse(value));
                config.ToByteArrayFunction = value => BitConverter.GetBytes(value);
            });

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ICIdDefinitionRegistry>();

        Assert.Equal(typeof(Guid), registry.Get(typeof(CustomerEntity)).ValueType);
        Assert.Equal(typeof(int), registry.Get(typeof(LegacyEntity)).ValueType);
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
}

