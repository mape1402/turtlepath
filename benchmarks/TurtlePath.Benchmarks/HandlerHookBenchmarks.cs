using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TurtlePath.Hooks;

namespace TurtlePath.Benchmarks;

public class HandlerHookBenchmarks
{
    private ServiceProvider _provider;
    private IHandlerHookRunner _runner;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddTurtlePath();
        services.AddHandlerHook<NoOpHook>();
        _provider = services.BuildServiceProvider();
        _runner = _provider.GetRequiredService<IHandlerHookRunner>();
    }

    [Benchmark]
    public ValueTask RunSingleHook()
        => _runner.RunAsync<INoOpHook>(hook => hook.RunAsync());

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

