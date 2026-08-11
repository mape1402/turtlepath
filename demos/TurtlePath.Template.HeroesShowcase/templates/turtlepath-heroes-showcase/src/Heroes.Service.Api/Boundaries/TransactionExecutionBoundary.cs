using Spider.Pipelines.Boundaries;
using System.Transactions;

namespace Heroes.Service.Api.Boundaries
{
    /// <summary>
    /// Wraps Spider pipeline executions in an ambient transaction.
    /// </summary>
    public sealed class TransactionExecutionBoundary : PipelineExecutionBoundary
    {
        private const string _scopeKey = "Heroes.Service.TransactionExecutionBoundary.Scope";
        private readonly ITransactionBoundaryRequestFilter _requestFilter;
        private readonly TransactionBoundaryOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionExecutionBoundary"/> class.
        /// </summary>
        /// <param name="_options">The transaction boundary _options.</param>
        /// <param name="_requestFilter">The cached request filter.</param>
        public TransactionExecutionBoundary(
            Microsoft.Extensions.Options.IOptions<TransactionBoundaryOptions> _options,
            ITransactionBoundaryRequestFilter _requestFilter)
        {
            this._options = _options?.Value ?? new TransactionBoundaryOptions();
            this._requestFilter = _requestFilter ?? throw new ArgumentNullException(nameof(_requestFilter));
        }

        /// <inheritdoc />
        public override ValueTask BeginAsync(PipelineExecutionContext context, CancellationToken cancellationToken)
        {
            if (!ShouldOpenTransaction(context))
                return ValueTask.CompletedTask;

            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = _options.IsolationLevel,
                Timeout = _options.TimeoutSeconds.HasValue
                    ? TimeSpan.FromSeconds(_options.TimeoutSeconds.Value)
                    : TransactionManager.MaximumTimeout
            };

            context.Items[_scopeKey] = new TransactionScope(
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
            if (context.Items.TryGetValue(_scopeKey, out var value) && value is TransactionScope transactionScope)
            {
                context.Items.Remove(_scopeKey);
                scope = transactionScope;
                return true;
            }

            scope = null;
            return false;
        }

        private bool ShouldOpenTransaction(PipelineExecutionContext context)
            => _requestFilter.ShouldOpenTransaction(context?.RequestType);
    }
}
