using Spider.Pipelines.Boundaries;
using System.Transactions;

namespace DTemplate.Api.Boundaries
{
    /// <summary>
    /// Wraps Spider pipeline executions in an ambient transaction.
    /// </summary>
    public sealed class TransactionExecutionBoundary : PipelineExecutionBoundary
    {
        private const string ScopeKey = "DTemplate.TransactionExecutionBoundary.Scope";
        private readonly ITransactionBoundaryRequestFilter requestFilter;
        private readonly TransactionBoundaryOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionExecutionBoundary"/> class.
        /// </summary>
        /// <param name="options">The transaction boundary options.</param>
        /// <param name="requestFilter">The cached request filter.</param>
        public TransactionExecutionBoundary(
            Microsoft.Extensions.Options.IOptions<TransactionBoundaryOptions> options,
            ITransactionBoundaryRequestFilter requestFilter)
        {
            this.options = options?.Value ?? new TransactionBoundaryOptions();
            this.requestFilter = requestFilter ?? throw new ArgumentNullException(nameof(requestFilter));
        }

        /// <inheritdoc />
        public override ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            if (!ShouldOpenTransaction(context))
                return ValueTask.CompletedTask;

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = options.IsolationLevel,
                Timeout = options.TimeoutSeconds.HasValue
                    ? TimeSpan.FromSeconds(options.TimeoutSeconds.Value)
                    : TransactionManager.MaximumTimeout
            };

            context.Items[ScopeKey] = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override ValueTask CompleteAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            if (TryRemoveScope(context, out var scope))
            {
                scope.Complete();
                scope.Dispose();
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override ValueTask FaultAsync(PipelineExecutionContext context, Exception exception, CancellationToken cancellationToken)
        {
            DisposeScope(context);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override ValueTask CancelAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            DisposeScope(context);

            return ValueTask.CompletedTask;
        }

        private static void DisposeScope(PipelineExecutionContext context)
        {
            if (TryRemoveScope(context, out var scope))
                scope.Dispose();
        }

        private static bool TryRemoveScope(PipelineExecutionContext context, out TransactionScope scope)
        {
            if (context.Items.TryGetValue(ScopeKey, out var value) && value is TransactionScope transactionScope)
            {
                context.Items.Remove(ScopeKey);
                scope = transactionScope;
                return true;
            }

            scope = null;
            return false;
        }

        private bool ShouldOpenTransaction(PipelineExecutionContext context)
            => requestFilter.ShouldOpenTransaction(context?.RequestType);
    }
}
