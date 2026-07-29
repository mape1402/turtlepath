using Pelican.Mediator;

namespace TurtlePath.Application.Commands
{
    /// <summary>
    /// Provides a base implementation for handling commands with a request and response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the command request and returns a response asynchronously.
        /// </summary>
        /// <param name="request">The command request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response as the result.</returns>
        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
