using Pelican.Mediator;

namespace TurtlePath.Application.Commands
{
    /// <summary>
    /// Provides a base implementation for handling commands that do not return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    public abstract class NoReturnCommandHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
    {
        /// <summary>
        /// Handles the command request asynchronously.
        /// </summary>
        /// <param name="request">The command request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
