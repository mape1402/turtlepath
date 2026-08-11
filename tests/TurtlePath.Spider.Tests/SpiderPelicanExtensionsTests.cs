using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Spider;

namespace TurtlePath.Spider.Tests;

public sealed class SpiderPelicanExtensionsTests
{
    [Fact]
    public async Task DefaultSend_dispatches_response_request_through_pelican()
    {
        var services = new ServiceCollection();

        services.AddPelican(typeof(SpiderPelicanExtensionsTests).Assembly);
        services.AddSpider();

        using var provider = services.BuildServiceProvider();
        var spider = provider.GetRequiredService<ISpider>();

        var response = await spider.DefaultSend<PingRequest, PingResponse>(new PingRequest("alive"));

        Assert.Equal("pong:alive", response.Message);
    }

    [Fact]
    public async Task DefaultSend_dispatches_no_response_request_through_pelican()
    {
        var services = new ServiceCollection();

        services.AddSingleton<PingSink>();
        services.AddPelican(typeof(SpiderPelicanExtensionsTests).Assembly);
        services.AddSpider();

        using var provider = services.BuildServiceProvider();
        var spider = provider.GetRequiredService<ISpider>();

        await spider.DefaultSend(new TrackPingRequest("tracked"));

        Assert.Equal("tracked", provider.GetRequiredService<PingSink>().LastMessage);
    }

    public sealed record PingRequest(string Message) : IRequest<PingResponse>;

    public sealed record PingResponse(string Message);

    public sealed class PingRequestHandler : IRequestHandler<PingRequest, PingResponse>
    {
        public Task<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new PingResponse($"pong:{request.Message}"));
    }

    public sealed record TrackPingRequest(string Message) : IRequest;

    public sealed class PingSink
    {
        public string LastMessage { get; set; } = string.Empty;
    }

    public sealed class TrackPingRequestHandler : IRequestHandler<TrackPingRequest>
    {
        private readonly PingSink sink;

        public TrackPingRequestHandler(PingSink sink)
        {
            this.sink = sink;
        }

        public Task Handle(TrackPingRequest request, CancellationToken cancellationToken = default)
        {
            sink.LastMessage = request.Message;
            return Task.CompletedTask;
        }
    }
}
