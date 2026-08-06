using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Runs all registered recurring TurtlePath cron jobs.
    /// </summary>
    public sealed class TurtlePathCronJobHostedService : BackgroundService
    {
        private readonly IEnumerable<TurtlePathCronJobDefinition> cronJobDefinitions;
        private readonly ITurtlePathJobExecutor jobExecutor;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TurtlePathCronJobHostedService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurtlePathCronJobHostedService"/> class.
        /// </summary>
        public TurtlePathCronJobHostedService(
            IEnumerable<TurtlePathCronJobDefinition> cronJobDefinitions,
            ITurtlePathJobExecutor jobExecutor,
            IServiceProvider serviceProvider,
            ILogger<TurtlePathCronJobHostedService> logger)
        {
            this.cronJobDefinitions = cronJobDefinitions ?? throw new ArgumentNullException(nameof(cronJobDefinitions));
            this.jobExecutor = jobExecutor ?? throw new ArgumentNullException(nameof(jobExecutor));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var definitions = cronJobDefinitions.ToArray();

            if (definitions.Length == 0)
                return;

            await Task.WhenAll(definitions.Select(definition => RunLoopAsync(definition, stoppingToken))).ConfigureAwait(false);
        }

        private async Task RunLoopAsync(TurtlePathCronJobDefinition definition, CancellationToken stoppingToken)
        {
            if (!definition.Options.RunOnStart)
                await DelayAsync(definition.Options.Interval, stoppingToken).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await jobExecutor
                    .ExecuteAsync(definition.JobType, definition.Name, definition.Options, stoppingToken)
                    .ConfigureAwait(false);

                if (!result.Succeeded)
                {
                    logger.LogError(
                        result.Exception?.Exception,
                        "TurtlePath cron job {JobName} failed after {Attempts} attempts.",
                        result.JobName,
                        result.Attempts);

                    if (definition.Options.FailureBehavior == TurtlePathJobFailureBehavior.Rethrow)
                        throw result.Exception?.Exception ?? new TurtlePathJobManagerException(new TurtlePathJobManagerResult { Jobs = [ result ] });

                    if (definition.Options.FailureBehavior == TurtlePathJobFailureBehavior.StopHost)
                    {
                        serviceProvider.GetService<IHostApplicationLifetime>()?.StopApplication();
                        return;
                    }
                }

                await DelayAsync(definition.Options.Interval, stoppingToken).ConfigureAwait(false);
            }
        }

        private static async Task DelayAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            if (interval <= TimeSpan.Zero)
                return;

            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }
}
