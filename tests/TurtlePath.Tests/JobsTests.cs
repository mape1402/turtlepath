using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TurtlePath.ExceptionHandling;
using TurtlePath.Jobs;

namespace TurtlePath.Tests;

public sealed class JobsTests
{
    [Fact]
    public async Task Manager_runs_registered_one_shot_jobs_in_parallel()
    {
        ParallelJobState.Reset();
        var services = CreateServices();

        services
            .AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Parallel;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Continue;
            })
            .AddJob<FirstParallelJob>()
            .AddJob<SecondParallelJob>();

        using var provider = services.BuildServiceProvider();
        var result = await provider.RunTurtlePathJobsAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Jobs.Count);
        Assert.Equal(2, ParallelJobState.Started);
        Assert.Equal(2, ParallelJobState.Completed);
    }

    [Fact]
    public async Task Manager_retries_failed_jobs()
    {
        RetryJob.Attempts = 0;
        var services = CreateServices();

        services
            .AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Sequential;
                options.Retries = 2;
            })
            .AddJob<RetryJob>();

        using var provider = services.BuildServiceProvider();
        var result = await provider.RunTurtlePathJobsAsync();
        var job = Assert.Single(result.Jobs);

        Assert.True(result.Succeeded);
        Assert.Equal(2, job.Attempts);
        Assert.Equal(2, RetryJob.Attempts);
    }

    [Fact]
    public async Task Manager_throws_aggregate_exception_when_configured_to_rethrow()
    {
        var services = CreateServices();

        services
            .AddTurtlePathJobs(options =>
            {
                options.ExecutionMode = TurtlePathJobExecutionMode.Sequential;
                options.FailureBehavior = TurtlePathJobFailureBehavior.Rethrow;
            })
            .AddJob<FailingJob>();

        using var provider = services.BuildServiceProvider();

        var exception = await Assert.ThrowsAsync<TurtlePathJobManagerException>(() => provider.RunTurtlePathJobsAsync());
        var result = Assert.Single(exception.Result.Jobs);

        Assert.False(result.Succeeded);
        Assert.Equal(ExceptionKind.Failure, result.Exception.Kind);
    }

    [Fact]
    public void Services_register_multiple_cron_jobs()
    {
        var services = CreateServices();

        services
            .AddTurtlePathJobs()
            .AddCronJob<FirstParallelJob>(options => options.EverySeconds(30))
            .AddCronJob<SecondParallelJob>(options => options.EveryMinutes(5));

        using var provider = services.BuildServiceProvider();
        var definitions = provider.GetServices<TurtlePathCronJobDefinition>().ToArray();

        Assert.Equal(2, definitions.Length);
        Assert.Contains(provider.GetServices<IHostedService>(), service => service is TurtlePathCronJobHostedService);
        Assert.Contains(definitions, definition => definition.JobType == typeof(FirstParallelJob) && definition.Options.Interval == TimeSpan.FromSeconds(30));
        Assert.Contains(definitions, definition => definition.JobType == typeof(SecondParallelJob) && definition.Options.Interval == TimeSpan.FromMinutes(5));
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddTurtlePathExceptionHandlingCore();

        return services;
    }

    private static class ParallelJobState
    {
        private static TaskCompletionSource bothStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public static int Started;

        public static int Completed;

        public static void Reset()
        {
            Started = 0;
            Completed = 0;
            bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public static async Task MarkStartedAndWaitAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref Started) == 2)
                bothStarted.TrySetResult();

            await bothStarted.Task.WaitAsync(cancellationToken);
            Interlocked.Increment(ref Completed);
        }
    }

    private sealed class FirstParallelJob : TurtlePathJob
    {
        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
            => ParallelJobState.MarkStartedAndWaitAsync(cancellationToken);
    }

    private sealed class SecondParallelJob : TurtlePathJob
    {
        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
            => ParallelJobState.MarkStartedAndWaitAsync(cancellationToken);
    }

    private sealed class RetryJob : TurtlePathJob
    {
        public static int Attempts;

        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
        {
            Attempts++;

            if (Attempts == 1)
                throw new InvalidOperationException("Retry me.");

            return Task.CompletedTask;
        }
    }

    private sealed class FailingJob : TurtlePathJob
    {
        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Always fails.");
    }
}
