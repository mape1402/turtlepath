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
    public void Constructor_rejects_values_outside_configured_type()
    {
        CIdMetadata.Reset();
        var services = new ServiceCollection();
        services.UseCId<Guid, string>(config =>
        {
            config.DefaultFactory = () => new CId(Guid.NewGuid());
            config.ConvertToDb = id => id.ToString();
            config.ConvertFromDb = value => new CId(Guid.Parse(value));
            config.JsonConverter = value => new CId(Guid.Parse(value));
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : new CId(Guid.Parse(value));
            config.ParseFunction = value => new CId(Guid.Parse(value));
            config.ToByteArrayFunction = value => value.ToByteArray();
        });

        Assert.Throws<ArgumentException>(() => new CId("not-a-guid"));
    }
}
