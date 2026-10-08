using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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

        var response = await client.PostAsJsonAsync("/api/v1/customers", new CreateCustomerCommand
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

        var response = await client.PutAsJsonAsync("/api/v1/customers/customer-42", new UpdateCustomerCommand
        {
            Name = "luigi"
        });

        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("luigi", body.Name);
        Assert.Equal(CId.From("customer-42"), app.Services.GetRequiredService<CustomerSink>().LastId);
    }

    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_applies_request_binder_after_body_binding()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/customers/customer-99/custom")
        {
            Content = JsonContent.Create(new CustomRouteCustomerCommand
            {
                Name = "peach"
            })
        };
        request.Headers.Add("X-Source", "route-factory");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        var sink = app.Services.GetRequiredService<CustomerSink>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("peach", body.Name);
        Assert.Equal(CId.From("customer-99"), sink.LastId);
        Assert.Equal("route-factory", sink.LastSource);
    }

    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_uses_binding_values_to_create_query()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/api/v1/customers/customer-77/by-alt?source=query-binding");
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        var sink = app.Services.GetRequiredService<CustomerSink>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("alt", body.Name);
        Assert.Equal(CId.From("customer-77"), sink.LastId);
        Assert.Equal("query-binding", sink.LastSource);
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

    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_exposes_routes_to_api_explorer()
    {
        await using var app = await CreateAppAsync();

        var descriptions = app.Services
            .GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups
            .Items
            .SelectMany(group => group.Items)
            .ToList();

        Assert.Contains(descriptions, item =>
            item.HttpMethod == "POST" &&
            item.RelativePath == "api/v1/customers" &&
            item.GroupName == "v1");
        Assert.Contains(descriptions, item =>
            item.HttpMethod == "PUT" &&
            item.RelativePath == "api/v1/customers/{id}" &&
            item.GroupName == "v1");
    }

    [Fact]
    public async Task MapTurtlePathAutomationEndpoints_applies_entity_and_endpoint_metadata()
    {
        await using var app = await CreateAppAsync();

        var endpoints = app.Services
            .GetRequiredService<IEnumerable<EndpointDataSource>>()
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();

        var create = Assert.Single(endpoints, item => item.RoutePattern.RawText == "api/v1/secured-customers");
        var custom = Assert.Single(endpoints, item => item.RoutePattern.RawText == "api/v1/secured-customers/{customerId}/custom");

        Assert.Contains(create.Metadata.GetOrderedMetadata<IAuthorizeData>(), item => item.Policy == "customers");
        Assert.Contains(custom.Metadata.GetOrderedMetadata<IAuthorizeData>(), item => item.Policy == "customers");
        Assert.Contains(custom.Metadata.GetOrderedMetadata<IAuthorizeData>(), item => item.Policy == "customers.custom");
    }

    private static async Task<WebApplication> CreateAppAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<CustomerSink>();
        builder.Services.AddPelican(typeof(AutomationEndpointRouteBuilderTests).Assembly);

        var app = builder.Build();
        app.MapTurtlePathAutomationEndpoints(
            descriptors: AutomationDescriptorDiscovery.Discover(typeof(AutomationEndpointRouteBuilderTests).Assembly),
            configure: options => options.RoutePrefix = "api/v1");

        await app.StartAsync();
        return app;
    }

    private sealed class Customer : BaseEntity
    {
        public string Name { get; set; }
    }

    private sealed class SecuredCustomer : BaseEntity
    {
        public string Name { get; set; }
    }

    public sealed class SecuredCreateCustomerCommand : IRequest<CustomerResponse>
    {
        public string Name { get; set; }
    }

    public sealed class SecuredUpdateCustomerCommand : BaseRequest, IRequest<CustomerResponse>
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

    public sealed class CustomRouteCustomerCommand : BaseRequest, IRequest<CustomerResponse>
    {
        public string Name { get; set; }

        public string Source { get; set; }
    }

    public sealed class GetCustomerByAltIdQuery : IRequest<CustomerResponse>
    {
        public GetCustomerByAltIdQuery(CId id, string source)
        {
            Id = id;
            Source = source;
        }

        public CId Id { get; }

        public string Source { get; }
    }

    private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
    {
        public override void Configure(ITurtlePathAutomationBuilder builder)
        {
            builder.For<Customer>()
                .ToCreate<CreateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint("customers", name: "CreateCustomer"))
                .ToUpdate<UpdateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint("customers/{id}", name: "UpdateCustomer"))
                .ToUpdate<CustomRouteCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint(
                        "customers/{customerId}/custom",
                        endpoint => endpoint
                            .Name("CustomRouteCustomer")
                            .Bind((request, context) =>
                            {
                                request.Id = context.GetRouteParam<CId>("customerId");
                                request.Source = context.GetHeader("X-Source");
                            })))
                .ToGetById<GetCustomerByAltIdQuery, CustomerResponse>(operation => operation
                    .Endpoint(
                        "customers/{customerId}/by-alt",
                        endpoint => endpoint
                            .Name("GetCustomerByAltId")
                            .Bind(context => new
                            {
                                Id = context.GetRouteParam<CId>("customerId"),
                                Source = context.GetQuery("source")
                            })));
        }
    }

    private sealed class SecuredCustomerAutomationProfile : TurtlePathAutomationProfile
    {
        public override void Configure(ITurtlePathAutomationBuilder builder)
        {
            builder.For<SecuredCustomer>()
                .Endpoints(endpoint => endpoint.Authorize("customers"))
                .ToCreate<SecuredCreateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint("secured-customers", name: "SecuredCreateCustomer"))
                .ToUpdate<SecuredUpdateCustomerCommand, CustomerResponse>(operation => operation
                    .Endpoint(
                        "secured-customers/{customerId}/custom",
                        endpoint => endpoint
                            .Name("SecuredCustomRouteCustomer")
                            .Authorize("customers.custom")));
        }
    }

    public sealed class CustomerSink
    {
        public List<string> Names { get; } = [];

        public CId LastId { get; set; }

        public string LastSource { get; set; }
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

    public sealed class CustomRouteCustomerCommandHandler : IRequestHandler<CustomRouteCustomerCommand, CustomerResponse>
    {
        private readonly CustomerSink sink;

        public CustomRouteCustomerCommandHandler(CustomerSink sink)
        {
            this.sink = sink;
        }

        public Task<CustomerResponse> Handle(CustomRouteCustomerCommand request, CancellationToken cancellationToken = default)
        {
            sink.LastId = request.Id;
            sink.LastSource = request.Source;

            return Task.FromResult(new CustomerResponse
            {
                Id = request.Id,
                Name = request.Name
            });
        }
    }

    public sealed class GetCustomerByAltIdQueryHandler : IRequestHandler<GetCustomerByAltIdQuery, CustomerResponse>
    {
        private readonly CustomerSink sink;

        public GetCustomerByAltIdQueryHandler(CustomerSink sink)
        {
            this.sink = sink;
        }

        public Task<CustomerResponse> Handle(GetCustomerByAltIdQuery request, CancellationToken cancellationToken = default)
        {
            sink.LastId = request.Id;
            sink.LastSource = request.Source;

            return Task.FromResult(new CustomerResponse
            {
                Id = request.Id,
                Name = "alt"
            });
        }
    }
}
