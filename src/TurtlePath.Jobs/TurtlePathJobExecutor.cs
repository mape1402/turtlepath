using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.Workers;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Executes a single TurtlePath job using retries and exception handling.
    /// </summary>
    public sealed class TurtlePathJobExecutor : ITurtlePathJobExecutor
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IBackgroundExceptionBoundary exceptionBoundary;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TurtlePathJobExecutor> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurtlePathJobExecutor"/> class.
        /// </summary>
        public TurtlePathJobExecutor(
            IServiceScopeFactory scopeFactory,
            IBackgroundExceptionBoundary exceptionBoundary,
            IExceptionHandler exceptionHandler,
            IServiceProvider serviceProvider,
            ILogger<TurtlePathJobExecutor> logger)
        {
            this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            this.exceptionBoundary = exceptionBoundary ?? throw new ArgumentNullException(nameof(exceptionBoundary));
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<TurtlePathJobResult> ExecuteAsync(
            Type jobType,
            string jobName,
            TurtlePathJobExecutionOptions options,
            CancellationToken cancellationToken = default)
        {
            if (jobType == null)
                throw new ArgumentNullException(nameof(jobType));

            if (!typeof(ITurtlePathJob).IsAssignableFrom(jobType))
                throw new ArgumentException($"Type '{jobType.FullName}' must implement {nameof(ITurtlePathJob)}.", nameof(jobType));

            options ??= new TurtlePathJobExecutionOptions();
            jobName ??= jobType.Name;

            var stopwatch = Stopwatch.StartNew();
            var attempts = Math.Max(0, options.Retries) + 1;

            for (var attempt = 1; attempt <= attempts; attempt++)
            {
                var context = new TurtlePathJobContext
                {
                    JobName = jobName
                }.ForAttempt(attempt);

                try
                {
                    await exceptionBoundary.RunAsync(
                        token => ExecuteJobAsync(jobType, context, token),
                        new BackgroundExceptionContext
                        {
                            Workload = jobName,
                            TraceIdentifier = context.ExecutionId,
                            Items = context.Items
                        },
                        cancellationToken).ConfigureAwait(false);

                    stopwatch.Stop();

                    return new TurtlePathJobResult
                    {
                        JobName = jobName,
                        JobType = jobType,
                        Succeeded = true,
                        Attempts = attempt,
                        Duration = stopwatch.Elapsed
                    };
                }
                catch (Exception exception) when (attempt < attempts && !cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning(
                        exception,
                        "TurtlePath job {JobName} failed on attempt {Attempt}. Retrying.",
                        jobName,
                        attempt);

                    if (options.RetryDelay > TimeSpan.Zero)
                        await Task.Delay(options.RetryDelay, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    stopwatch.Stop();
                    var descriptor = exceptionHandler.Handle(
                        exception,
                        new ExceptionHandlingContext
                        {
                            TraceIdentifier = context.ExecutionId,
                            Items = context.Items
                        });

                    var result = new TurtlePathJobResult
                    {
                        JobName = jobName,
                        JobType = jobType,
                        Succeeded = false,
                        Attempts = attempt,
                        Duration = stopwatch.Elapsed,
                        Exception = descriptor
                    };

                    if (options.FailureBehavior == TurtlePathJobFailureBehavior.StopHost)
                        serviceProvider.GetService<IHostApplicationLifetime>()?.StopApplication();

                    return result;
                }
            }

            throw new InvalidOperationException($"Job '{jobName}' did not produce an execution result.");
        }

        private async Task ExecuteJobAsync(Type jobType, TurtlePathJobContext context, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var job = (ITurtlePathJob)scope.ServiceProvider.GetRequiredService(jobType);

            await job.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
