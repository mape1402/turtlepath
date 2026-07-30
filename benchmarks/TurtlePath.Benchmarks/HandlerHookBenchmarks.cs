using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Hooks;

namespace TurtlePath.Benchmarks;

public class HandlerHookBenchmarks
{
    private ServiceProvider _provider;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddHandlerHook<NoOpHook>();
        _provider = services.BuildServiceProvider();
    }

    [Benchmark]
    public ValueTask RunSingleHook()
        => _provider.RunHooksAsync<INoOpHook>(hook => hook.RunAsync());

    private interface INoOpHook : IOrderedHook
    {
        ValueTask RunAsync();
    }

    private sealed class NoOpHook : INoOpHook
    {
        public int Order => 0;

        public ValueTask RunAsync()
            => ValueTask.CompletedTask;
    }
}

