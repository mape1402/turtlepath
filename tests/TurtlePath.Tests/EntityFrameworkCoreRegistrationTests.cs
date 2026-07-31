using Microsoft.Extensions.DependencyInjection;
using TurtlePath.EntityFrameworkCore;

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
}
