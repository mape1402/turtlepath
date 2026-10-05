namespace TurtlePath.Automations.Tests
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    public sealed class TurtlePathAutomationArchitectureTests
    {
        [Fact]
        public void BuildManifest_includes_concrete_automation_operations()
        {
            var manifest = TurtlePathAutomationArchitecture.BuildManifest(typeof(TurtlePathAutomationArchitectureTests).Assembly);

            var create = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "CreateArchitectureCustomerCommand -> ArchitectureCustomerResponse");
            Assert.Equal("create", create.Metadata["operation"]);
            Assert.Equal("False", create.Metadata["validation.enabled"]);
            Assert.Equal("True", create.Metadata["projection.reload-before-response"]);

            var paged = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "SearchArchitectureCustomersQuery -> PagedResponse<ArchitectureCustomerResponse>");
            Assert.Equal("getpaged", paged.Metadata["operation"]);
            Assert.Equal("-Name", paged.Metadata["query.default-sort"]);

            Assert.Contains(manifest.Relations, relation => relation.SourceId == create.Id && relation.TargetId == "turtlepath.command.create" && relation.Kind == "implements");
            Assert.Contains(manifest.Components, component => component.Id == $"{create.Id}.response" && component.Metadata["tags"].Contains("reload"));
        }

        private sealed class ArchitectureAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<ArchitectureCustomer>()
                    .ToCreate<CreateArchitectureCustomerCommand, ArchitectureCustomerResponse>(mutation => mutation
                        .ValidateRequest(false)
                        .Include(customer => customer.Parent))
                    .ToGetPaged<SearchArchitectureCustomersQuery, ArchitectureCustomerResponse>(query => query.DefaultSort("-Name"));
            }
        }

        private sealed class ArchitectureCustomer : BaseEntity
        {
            public ArchitectureCustomer Parent { get; set; }
        }

        private sealed class ArchitectureCustomerResponse : IBaseResponse<CId>
        {
            public CId Id { get; set; }
        }

        private sealed class CreateArchitectureCustomerCommand : IRequest<ArchitectureCustomerResponse>
        {
        }

        private sealed class SearchArchitectureCustomersQuery : IRequest<PagedResponse<ArchitectureCustomerResponse>>
        {
        }
    }
}
