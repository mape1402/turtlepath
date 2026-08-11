using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Spider;

namespace Heroes.Service.Tests;

/// <summary>
/// Verifies the small bridge used by controllers to execute Pelican handlers through Spider.
/// </summary>
public sealed class SpiderPelicanBridgeTests
{
    /// <summary>
    /// Ensures response-based requests can travel through Spider and land in Pelican.
    /// </summary>
    [Fact]
    public async Task DefaultSend_dispatches_response_request_through_pelican()
    {
        var services = new ServiceCollection();

        services.AddPelican(typeof(SpiderPelicanBridgeTests).Assembly);
        services.AddSpider();

        using var provider = services.BuildServiceProvider();
        var spider = provider.GetRequiredService<ISpider>();

        var response = await spider.DefaultSend<PingRequest, PingResponse>(new PingRequest("alive"));

        Assert.Equal("pong:alive", response.Message);
    }

    /// <summary>
    /// Ensures no-response commands can travel through the same bridge.
    /// </summary>
    [Fact]
    public async Task DefaultSend_dispatches_no_response_request_through_pelican()
    {
        var services = new ServiceCollection();

        services.AddSingleton<PingSink>();
        services.AddPelican(typeof(SpiderPelicanBridgeTests).Assembly);
        services.AddSpider();

        using var provider = services.BuildServiceProvider();
        var spider = provider.GetRequiredService<ISpider>();

        await spider.DefaultSend(new TrackPingRequest("tracked"));

        Assert.Equal("tracked", provider.GetRequiredService<PingSink>().LastMessage);
    }

    /// <summary>
    /// Request used to test response-based Pelican dispatch.
    /// </summary>
    public sealed record PingRequest(string Message) : IRequest<PingResponse>;

    /// <summary>
    /// Response returned by the test handler.
    /// </summary>
    public sealed record PingResponse(string Message);

    /// <summary>
    /// Handler resolved by Pelican through the Spider bridge.
    /// </summary>
    public sealed class PingRequestHandler : IRequestHandler<PingRequest, PingResponse>
    {
        /// <inheritdoc />
        public Task<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PingResponse($"pong:{request.Message}"));
    }

    /// <summary>
    /// No-response request used to test command dispatch.
    /// </summary>
    public sealed record TrackPingRequest(string Message) : IRequest;

    /// <summary>
    /// Captures handler side effects for the no-response bridge test.
    /// </summary>
    public sealed class PingSink
    {
        /// <summary>
        /// Gets or sets the last message captured by the handler.
        /// </summary>
        public string LastMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler resolved by Pelican for no-response command dispatch.
    /// </summary>
    public sealed class TrackPingRequestHandler : IRequestHandler<TrackPingRequest>
    {
        private readonly PingSink _sink;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackPingRequestHandler"/> class.
        /// </summary>
        public TrackPingRequestHandler(PingSink _sink)
        {
            this._sink = _sink;
        }

        /// <inheritdoc />
        public Task Handle(TrackPingRequest request, CancellationToken cancellationToken = default)
        {
            _sink.LastMessage = request.Message;
            return Task.CompletedTask;
        }
    }
}
