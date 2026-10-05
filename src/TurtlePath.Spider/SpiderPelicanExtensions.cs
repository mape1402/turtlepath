using Pelican.Mediator;
using Spider.Pipelines.Core;
using Spider.Pipelines.Extensions;

namespace TurtlePath.Spider;

/// <summary>
/// Provides TurtlePath bridge helpers for dispatching Pelican requests through Spider pipelines.
/// </summary>
public static class SpiderPelicanExtensions
{
    /// <summary>
    /// Creates a Pelican mediator bridge from a Spider pipeline instance.
    /// </summary>
    /// <param name="spider">The Spider pipeline instance.</param>
    /// <returns>A Spider service bridge for <see cref="IMediator"/>.</returns>
    public static IServiceBridge<IMediator> AsMediator(this ISpider spider)
    {
        ArgumentNullException.ThrowIfNull(spider);

        return spider.InitBridge<IMediator>();
    }

    /// <summary>
    /// Sends a no-response Pelican request through an existing Spider mediator bridge.
    /// </summary>
    /// <typeparam name="TRequest">The concrete request type.</typeparam>
    /// <param name="bridge">The Spider mediator bridge.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task Send<TRequest>(
        this IServiceBridge<IMediator, TRequest> bridge,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentNullException.ThrowIfNull(request);

        return bridge.ExecuteAsync(mediator => mediator.Send, request, cancellationToken);
    }

    /// <summary>
    /// Sends a response-based Pelican request through an existing Spider mediator bridge.
    /// </summary>
    /// <typeparam name="TRequest">The concrete request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="bridge">The Spider mediator bridge.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with the response as the result.</returns>
    public static Task<TResponse> Send<TRequest, TResponse>(
        this IServiceBridge<IMediator, TRequest, TResponse> bridge,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(bridge);
        ArgumentNullException.ThrowIfNull(request);

        return bridge.ExecuteAsync(mediator => (message, token) => mediator.Send(message, token), request, cancellationToken);
    }

    /// <summary>
    /// Sends a response-based Pelican request through Spider using the concrete request type as the pipeline contract.
    /// </summary>
    /// <typeparam name="TRequest">The concrete request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="spider">The Spider pipeline instance.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation, with the response as the result.</returns>
    public static Task<TResponse> DefaultSend<TRequest, TResponse>(
        this ISpider spider,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(spider);
        ArgumentNullException.ThrowIfNull(request);

        return spider
            .AsMediator()
            .Attach<TRequest, TResponse>(pipeline => pipeline
                .Named("Pelican request dispatch")
                .Describe("Dispatches a Pelican request through the configured Spider pipeline.")
                .Tags("turtlepath", "pelican", "dispatch"))
            .Send(request, cancellationToken);
    }

    /// <summary>
    /// Sends a no-response Pelican request through Spider using the concrete request type as the pipeline contract.
    /// </summary>
    /// <typeparam name="TRequest">The concrete request type.</typeparam>
    /// <param name="spider">The Spider pipeline instance.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task DefaultSend<TRequest>(
        this ISpider spider,
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(spider);
        ArgumentNullException.ThrowIfNull(request);

        return spider
            .AsMediator()
            .Attach<TRequest>(pipeline => pipeline
                .Named("Pelican request dispatch")
                .Describe("Dispatches a no-response Pelican request through the configured Spider pipeline.")
                .Tags("turtlepath", "pelican", "dispatch"))
            .Send(request, cancellationToken);
    }
}
