using TurtlePath.Identifier;
using Microsoft.Extensions.DependencyInjection;

namespace TurtlePath.Tests;

public class CIdTests
{
    [Fact]
    public void New_uses_configured_factory()
    {
        CIdMetadata.Reset();
        var services = new ServiceCollection();

        services.UseCId<Guid, string>(config =>
        {
            config.DefaultFactory = () => new CId(Guid.Parse("f8cb21f2-35d7-419b-9f58-90d1c82154f0"));
            config.ConvertToDb = id => id.ToString();
            config.ConvertFromDb = value => new CId(Guid.Parse(value));
            config.JsonConverter = value => new CId(Guid.Parse(value));
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : new CId(Guid.Parse(value));
            config.ParseFunction = value => new CId(Guid.Parse(value));
            config.ToByteArrayFunction = value => value.ToByteArray();
        });

        var id = CId.New();

        Assert.Equal("f8cb21f2-35d7-419b-9f58-90d1c82154f0", id.ToString());
    }

    [Fact]
    public void Constructor_accepts_different_underlying_types()
    {
        CIdMetadata.Reset();

        Assert.Equal(42, CId.From(42).Cast<int>());
        Assert.Equal(Guid.Empty, CId.From(Guid.Empty).Cast<Guid>());
    }

    [Fact]
    public void Composite_ids_compare_by_part_name_and_value()
    {
        CIdMetadata.Reset();
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
            typeof(int),
            () => CId.From(0),
            value => CId.From(int.Parse(value)),
            id => id.ToString(),
            id => BitConverter.GetBytes(id.Cast<int>()),
            CIdGenerationStrategy.StoreGenerated));
        registry.Register(new CIdDefinition(
            "Order",
            typeof(Guid),
            () => CId.From(Guid.Parse("e76768cb-ece0-4985-901e-c4c0e434b3fb")),
            value => CId.From(Guid.Parse(value)),
            id => id.ToString(),
            id => id.Cast<Guid>().ToByteArray(),
            CIdGenerationStrategy.ClientGenerated));

        Assert.Equal(0, registry.New("Customer").Cast<int>());
        Assert.Equal("e76768cb-ece0-4985-901e-c4c0e434b3fb", registry.New("Order").ToString());
    }
}
