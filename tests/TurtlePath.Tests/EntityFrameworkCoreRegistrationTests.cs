using Microsoft.Extensions.DependencyInjection;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Tests;

public class EntityFrameworkCoreRegistrationTests
{
    [Fact]
    public void AddTurtlePathEntityFrameworkCore_registers_default_options()
    {
        var services = new ServiceCollection();

        services.AddTurtlePathEntityFrameworkCore();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.True(options.ApplyConfigurations);
        Assert.True(options.ApplyBaseEntityConventions);
        Assert.True(options.ApplyCIdConverters);
        Assert.Empty(options.ConfigurationAssemblies);
    }

    [Fact]
    public void AddTurtlePathEntityFrameworkCore_registers_configured_options()
    {
        var services = new ServiceCollection();

        services.AddTurtlePathEntityFrameworkCore(options => options with
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
    public void AddTurtlePathEntityFrameworkCore_uses_registered_identifier_definition()
    {
        CIdMetadata.Reset();
        var services = new ServiceCollection();

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
            .UseEntityFrameworkCore();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<TurtlePathDbContextOptions>();

        Assert.NotNull(options.CIdDefinition);
        Assert.Equal(typeof(string), options.CIdDefinition.DatabaseValueType);
        Assert.Equal("uniqueidentifier", options.CIdDefinition.DatabaseColumnType);
        Assert.True(options.CIdDefinition.HasDatabaseConversion);
    }
}
