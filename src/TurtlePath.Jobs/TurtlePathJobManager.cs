using Microsoft.Extensions.Options;

namespace TurtlePath.Jobs
{
    /// <summary>
    /// Executes registered one-shot jobs.
    /// </summary>
    public sealed class TurtlePathJobManager : ITurtlePathJobManager
    {
        private readonly IEnumerable<TurtlePathJobDefinition> jobDefinitions;
        private readonly ITurtlePathJobExecutor jobExecutor;
        private readonly TurtlePathJobManagerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TurtlePathJobManager"/> class.
        /// </summary>
        public TurtlePathJobManager(
            IEnumerable<TurtlePathJobDefinition> jobDefinitions,
            ITurtlePathJobExecutor jobExecutor,
            IOptions<TurtlePathJobManagerOptions> options)
        {
            this.jobDefinitions = jobDefinitions ?? throw new ArgumentNullException(nameof(jobDefinitions));
            this.jobExecutor = jobExecutor ?? throw new ArgumentNullException(nameof(jobExecutor));
            this.options = options?.Value ?? new TurtlePathJobManagerOptions();
        }

        /// <inheritdoc />
        public Task<TurtlePathJobManagerResult> RunAsync(CancellationToken cancellationToken = default)
        {
            return RunAsync(jobDefinitions.Select(definition => definition.JobType), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TurtlePathJobManagerResult> RunAsync(IEnumerable<Type> jobTypes, CancellationToken cancellationToken = default)
        {
            if (jobTypes == null)
                throw new ArgumentNullException(nameof(jobTypes));

            var requestedTypes = jobTypes.ToArray();
            var definitions = jobDefinitions
                .Where(definition => requestedTypes.Contains(definition.JobType))
                .ToArray();

            var results = options.ExecutionMode == TurtlePathJobExecutionMode.Sequential
                ? await RunSequentialAsync(definitions, cancellationToken).ConfigureAwait(false)
                : await RunParallelAsync(definitions, cancellationToken).ConfigureAwait(false);

            var result = new TurtlePathJobManagerResult
            {
                Jobs = results
            };

            if (!result.Succeeded && options.FailureBehavior == TurtlePathJobFailureBehavior.Rethrow)
                throw new TurtlePathJobManagerException(result);

            return result;
        }

        private async Task<IReadOnlyCollection<TurtlePathJobResult>> RunSequentialAsync(
            IEnumerable<TurtlePathJobDefinition> definitions,
            CancellationToken cancellationToken)
        {
            var results = new List<TurtlePathJobResult>();

            foreach (var definition in definitions)
                results.Add(await ExecuteAsync(definition, cancellationToken).ConfigureAwait(false));

            return results;
        }

        private async Task<IReadOnlyCollection<TurtlePathJobResult>> RunParallelAsync(
            IEnumerable<TurtlePathJobDefinition> definitions,
            CancellationToken cancellationToken)
        {
            using var semaphore = new SemaphoreSlim(Math.Max(1, options.MaxDegreeOfParallelism));

            var tasks = definitions.Select(async definition =>
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                try
                {
                    return await ExecuteAsync(definition, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private Task<TurtlePathJobResult> ExecuteAsync(TurtlePathJobDefinition definition, CancellationToken cancellationToken)
        {
            return jobExecutor.ExecuteAsync(definition.JobType, definition.Name, options, cancellationToken);
        }
    }
}
