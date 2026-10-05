using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
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
    public async Task Executor_validates_job_type_and_honors_retry_delay_and_stop_host()
    {
        RetryJob.Attempts = 0;
        var lifetime = new RecordingHostApplicationLifetime();
        var services = CreateServices()
            .AddSingleton<IHostApplicationLifetime>(lifetime);

        services
            .AddTurtlePathJobs()
            .AddJob<RetryJob>()
            .AddJob<FailingJob>();

        using var provider = services.BuildServiceProvider();
        var executor = provider.GetRequiredService<ITurtlePathJobExecutor>();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            executor.ExecuteAsync(null, null, null));
        var invalidType = await Assert.ThrowsAsync<ArgumentException>(() =>
            executor.ExecuteAsync(typeof(string), null, null));

        Assert.Equal("jobType", invalidType.ParamName);

        var retried = await executor.ExecuteAsync(
            typeof(RetryJob),
            null,
            new TurtlePathJobExecutionOptions
            {
                Retries = 1,
                RetryDelay = TimeSpan.FromMilliseconds(1)
            });
        var failed = await executor.ExecuteAsync(
            typeof(FailingJob),
            "stop-host",
            new TurtlePathJobExecutionOptions
            {
                FailureBehavior = TurtlePathJobFailureBehavior.StopHost
            });

        Assert.True(retried.Succeeded);
        Assert.Equal(2, RetryJob.Attempts);
        Assert.False(failed.Succeeded);
        Assert.True(lifetime.Stopped);
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
    public async Task Services_run_selected_jobs_and_validate_arguments()
    {
        SelectedJob.Executions = 0;
        SkippedJob.Executions = 0;
        var services = CreateServices();

        services
            .AddTurtlePathJobs()
            .AddJob<SelectedJob>("selected")
            .AddJob<SkippedJob>("skipped");

        using var provider = services.BuildServiceProvider();
        var result = await provider.RunTurtlePathJobsAsync([typeof(SelectedJob)]);

        Assert.True(result.Succeeded);
        Assert.Equal(1, SelectedJob.Executions);
        Assert.Equal(0, SkippedJob.Executions);
        Assert.Equal("selected", Assert.Single(result.Jobs).JobName);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.GetRequiredService<ITurtlePathJobManager>().RunAsync(null));
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null).AddTurtlePathJobs());
        Assert.Throws<ArgumentNullException>(() => ((ITurtlePathJobsBuilder)null).AddJob<SelectedJob>());
        Assert.Throws<ArgumentNullException>(() => ((ITurtlePathJobsBuilder)null).AddCronJob<SelectedJob>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((IServiceProvider)null).RunTurtlePathJobsAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((IServiceProvider)null).RunTurtlePathJobsAsync([typeof(SelectedJob)]));
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
        Assert.Throws<ArgumentOutOfRangeException>(() => new TurtlePathCronJobOptions().Every(TimeSpan.Zero));
        Assert.Equal(TimeSpan.FromHours(2), new TurtlePathCronJobOptions().EveryHours(2).Interval);
    }

    [Fact]
    public async Task Cron_host_returns_immediately_when_no_jobs_are_registered()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var executor = new RecordingJobExecutor();
        var service = new TurtlePathCronJobHostedService(
            [],
            executor,
            provider,
            NullLogger<TurtlePathCronJobHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(0, executor.Executions);
    }

    [Fact]
    public async Task Cron_host_runs_job_on_start_until_stopped()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var executor = new RecordingJobExecutor();
        var service = new TurtlePathCronJobHostedService(
            [
                new TurtlePathCronJobDefinition
                {
                    JobType = typeof(FirstParallelJob),
                    Name = "cron",
                    Options = new TurtlePathCronJobOptions
                    {
                        RunOnStart = true,
                        Interval = TimeSpan.FromHours(1)
                    }
                }
            ],
            executor,
            provider,
            NullLogger<TurtlePathCronJobHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await executor.FirstExecution.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(1, executor.Executions);
    }

    [Fact]
    public async Task Cron_host_waits_before_first_run_when_run_on_start_is_disabled()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var executor = new RecordingJobExecutor();
        var service = new TurtlePathCronJobHostedService(
            [
                new TurtlePathCronJobDefinition
                {
                    JobType = typeof(FirstParallelJob),
                    Name = "cron",
                    Options = new TurtlePathCronJobOptions
                    {
                        RunOnStart = false,
                        Interval = TimeSpan.FromMilliseconds(1)
                    }
                }
            ],
            executor,
            provider,
            NullLogger<TurtlePathCronJobHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await executor.FirstExecution.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        Assert.True(executor.Executions >= 1);
    }

    [Fact]
    public async Task Cron_host_stops_application_when_failure_behavior_is_stop_host()
    {
        var lifetime = new RecordingHostApplicationLifetime();
        using var provider = CreateServices()
            .AddSingleton<IHostApplicationLifetime>(lifetime)
            .BuildServiceProvider();
        var executor = new RecordingJobExecutor
        {
            Result = new TurtlePathJobResult
            {
                JobName = "cron",
                JobType = typeof(FailingJob),
                Succeeded = false,
                Attempts = 1,
                Exception = new ExceptionDescriptor
                {
                    Kind = ExceptionKind.Failure,
                    Messages = ["failed"],
                    Exception = new InvalidOperationException("failed")
                }
            }
        };
        var service = new TurtlePathCronJobHostedService(
            [
                new TurtlePathCronJobDefinition
                {
                    JobType = typeof(FailingJob),
                    Name = "cron",
                    Options = new TurtlePathCronJobOptions
                    {
                        RunOnStart = true,
                        FailureBehavior = TurtlePathJobFailureBehavior.StopHost
                    }
                }
            ],
            executor,
            provider,
            NullLogger<TurtlePathCronJobHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await executor.FirstExecution.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await service.StopAsync(CancellationToken.None);

        Assert.True(lifetime.Stopped);
    }

    [Fact]
    public async Task Cron_host_rethrows_failed_result_when_configured()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var failure = new InvalidOperationException("cron failed");
        var executor = new RecordingJobExecutor
        {
            Result = new TurtlePathJobResult
            {
                JobName = "cron",
                JobType = typeof(FailingJob),
                Succeeded = false,
                Attempts = 1,
                Exception = new ExceptionDescriptor
                {
                    Kind = ExceptionKind.Failure,
                    Messages = ["failed"],
                    Exception = failure
                }
            }
        };
        var service = new TurtlePathCronJobHostedService(
            [
                new TurtlePathCronJobDefinition
                {
                    JobType = typeof(FailingJob),
                    Name = "cron",
                    Options = new TurtlePathCronJobOptions
                    {
                        RunOnStart = true,
                        FailureBehavior = TurtlePathJobFailureBehavior.Rethrow
                    }
                }
            ],
            executor,
            provider,
            NullLogger<TurtlePathCronJobHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteTask);

        Assert.Same(failure, exception);
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

    private sealed class SelectedJob : TurtlePathJob
    {
        public static int Executions;

        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
        {
            Executions++;
            return Task.CompletedTask;
        }
    }

    private sealed class SkippedJob : TurtlePathJob
    {
        public static int Executions;

        public override Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
        {
            Executions++;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingJobExecutor : ITurtlePathJobExecutor
    {
        public TaskCompletionSource FirstExecution { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Executions { get; private set; }

        public TurtlePathJobResult Result { get; init; } = new()
        {
            JobName = "cron",
            JobType = typeof(FirstParallelJob),
            Succeeded = true,
            Attempts = 1
        };

        public Task<TurtlePathJobResult> ExecuteAsync(
            Type jobType,
            string jobName,
            TurtlePathJobExecutionOptions options,
            CancellationToken cancellationToken = default)
        {
            Executions++;
            FirstExecution.TrySetResult();

            return Task.FromResult(Result);
        }
    }

    private sealed class RecordingHostApplicationLifetime : IHostApplicationLifetime
    {
        public bool Stopped { get; private set; }

        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => CancellationToken.None;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication()
        {
            Stopped = true;
        }
    }
}
