using Microsoft.Extensions.DependencyInjection;
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
    public async Task RunHooksAsync_executes_hooks_by_order()
    {
        var calls = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(calls);
        services.AddHandlerHook<SecondOrderedBeforeValidationHook>();
        services.AddHandlerHook<FirstOrderedBeforeValidationHook>();

        using var provider = services.BuildServiceProvider();
        var context = new CommandHookContext<SampleRequest, SampleEntity>(new SampleRequest());

        await provider.RunHooksAsync<IBeforeValidationHook<SampleRequest, SampleEntity>>(
            hook => hook.BeforeValidationAsync(context));

        Assert.Equal(["first", "second"], calls);
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

