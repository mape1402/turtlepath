using Pelican.Mediator;
using System.Transactions;

namespace TurtlePath.Core.PipelineBehaviors
{
    /// <summary>
    /// Wraps request handling in a transaction scope.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    public sealed class TransactionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        /// <summary>
        /// Handles the request within a transaction scope.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="next">The delegate to invoke the next handler.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The response from the next handler.</returns>
        public async Task<TResponse> Handle(TRequest request, Handler<TResponse> next, CancellationToken cancellationToken = default)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            using (var transaction = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
            {
                var response = await next();

                transaction.Complete();

                return response;
            }
        }
    }
}
