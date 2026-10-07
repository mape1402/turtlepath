using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using TurtlePath.Automations.AspNetCore;
using TurtlePath.Automations.Profiles;
using TurtlePath.Domain.Contracts;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;
using TurtlePath.Models.Responses;

namespace TurtlePath.Automations.AspNetCore.Tests;

public sealed class AutomationEndpointRouteBuilderTests
{
    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_maps_post_body_and_dispatches_request()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/customers", new CreateCustomerCommand
        {
            Name = "mario"
        });

        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("mario", body.Name);
    }

    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_maps_route_id_to_request()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        var response = await client.PutAsJsonAsync("/customers/customer-42", new UpdateCustomerCommand
        {
            Name = "luigi"
        });

        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("luigi", body.Name);
        Assert.Equal(CId.From("customer-42"), app.Services.GetRequiredService<CustomerSink>().LastId);
    }

    [Fact]
    public void Endpoint_stores_endpoint_metadata_in_descriptor()
    {
        var descriptor = AutomationDescriptorDiscovery
            .Discover(typeof(AutomationEndpointRouteBuilderTests).Assembly)
            .Single(item => item.RequestType == typeof(UpdateCustomerCommand));

        var options = Assert.IsType<AutomationEndpointOptions>(
            descriptor.Metadata[AutomationEndpointMetadata.Endpoint]);

        Assert.Equal("customers/{id}", options.Route);
        Assert.Equal("UpdateCustomer", options.Name);
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<CustomerSink>();
        builder.Services.AddPelican(typeof(AutomationEndpointRouteBuilderTests).Assembly);

        var app = builder.Build();
        app.MapTurtlePathAutomationEndpoints(
            descriptors: AutomationDescriptorDiscovery.Discover(typeof(AutomationEndpointRouteBuilderTests).Assembly));

        await app.StartAsync();
        return app;
    }

    private sealed class Customer : BaseEntity
    {
        public string Name { get; set; }
    }

    public sealed class CustomerResponse : IBaseResponse<CId>
    {
        public CId Id { get; set; }

        public string Name { get; set; }
    }

    public sealed class CreateCustomerCommand : IRequest<CustomerResponse>
    {
        public string Name { get; set; }
    }

    public sealed class UpdateCustomerCommand : BaseRequest, IRequest<CustomerResponse>
    {
        public string Name { get; set; }
    }

    private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
    {
        public override void Configure(ITurtlePathAutomationBuilder builder)
        {
            builder.For<Customer>()
                .ToCreate<CreateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint("customers", name: "CreateCustomer"))
                .ToUpdate<UpdateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint("customers/{id}", name: "UpdateCustomer"));
        }
    }

    public sealed class CustomerSink
    {
        public List<string> Names { get; } = [];

        public CId LastId { get; set; }
    }

    public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponse>
    {
        private readonly CustomerSink sink;

        public CreateCustomerCommandHandler(CustomerSink sink)
        {
            this.sink = sink;
        }

        public Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken = default)
        {
            sink.Names.Add(request.Name);

            return Task.FromResult(new CustomerResponse
            {
                Id = CId.From("created"),
                Name = request.Name
            });
        }
    }

    public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerResponse>
    {
        private readonly CustomerSink sink;

        public UpdateCustomerCommandHandler(CustomerSink sink)
        {
            this.sink = sink;
        }

        public Task<CustomerResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken = default)
        {
            sink.LastId = request.Id;

            return Task.FromResult(new CustomerResponse
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }
}
