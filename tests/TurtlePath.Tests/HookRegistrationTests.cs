using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Domain.Identifier;
using TurtlePath.Hooks;

namespace TurtlePath.Tests;

public class HookRegistrationTests
{
    [Fact]
    public void AddHandlerHooksFromAssemblies_registers_discovered_hooks()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new List<string>());

        services.AddHandlerHooksFromAssemblies(typeof(SampleBeforeValidationHook).Assembly);

        using var provider = services.BuildServiceProvider();
        var hooks = provider.GetServices<IBeforeValidationHook<SampleRequest, SampleEntity>>().ToArray();

        Assert.Contains(hooks, hook => hook is SampleBeforeValidationHook);
    }

    [Fact]
    public async Task HandlerHookRunner_executes_hooks_by_order()
    {
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddTurtlePath();
        services.AddHandlerHook<SecondOrderedBeforeValidationHook>();
        services.AddHandlerHook<FirstOrderedBeforeValidationHook>();

        using var provider = services.BuildServiceProvider();
        var context = new CommandHookContext<SampleRequest, SampleEntity>(new SampleRequest());
        var runner = provider.GetRequiredService<IHandlerHookRunner>();

        await runner.RunAsync<IBeforeValidationHook<SampleRequest, SampleEntity>>(
            hook => hook.BeforeValidationAsync(context));

        Assert.Equal(["first", "second"], calls);
    }

    [Fact]
    public void AddTurtlePath_registers_identifier_configuration_and_discovered_hooks()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new List<string>());

        services
            .AddTurtlePath(typeof(SampleBeforeValidationHook).Assembly)
            .UseCId<Guid, string>(config =>
            {
                config.DefaultFactory = () => CId.From(Guid.Parse("f8cb21f2-35d7-419b-9f58-90d1c82154f0"));
                config.ConvertToDb = id => id.ToString();
                config.ConvertFromDb = value => CId.Parse(value);
                config.JsonConverter = value => CId.Parse(value);
                config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.Parse(value);
                config.ParseFunction = value => CId.From(Guid.Parse(value));
                config.ToByteArrayFunction = value => value.ToByteArray();
            });

        using var provider = services.BuildServiceProvider();
        var idFactory = provider.GetRequiredService<ICIdFactory>();
        var hooks = provider.GetServices<IBeforeValidationHook<SampleRequest, SampleEntity>>().ToArray();

        Assert.Equal("f8cb21f2-35d7-419b-9f58-90d1c82154f0", idFactory.New().ToString());
        Assert.Contains(hooks, hook => hook is SampleBeforeValidationHook);
    }

    private sealed class SampleRequest
    {
    }

    private sealed class SampleEntity
    {
    }

    private sealed class SampleBeforeValidationHook : IBeforeValidationHook<SampleRequest, SampleEntity>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<SampleRequest, SampleEntity> context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    private sealed class FirstOrderedBeforeValidationHook(List<string> calls) : IBeforeValidationHook<SampleRequest, SampleEntity>, IOrderedHook
    {
        public int Order => 10;

        public ValueTask BeforeValidationAsync(CommandHookContext<SampleRequest, SampleEntity> context, CancellationToken cancellationToken = default)
        {
            calls.Add("first");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class SecondOrderedBeforeValidationHook(List<string> calls) : IBeforeValidationHook<SampleRequest, SampleEntity>, IOrderedHook
    {
        public int Order => 20;

        public ValueTask BeforeValidationAsync(CommandHookContext<SampleRequest, SampleEntity> context, CancellationToken cancellationToken = default)
        {
            calls.Add("second");
            return ValueTask.CompletedTask;
        }
    }
}

