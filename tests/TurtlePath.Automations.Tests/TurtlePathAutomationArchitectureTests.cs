namespace TurtlePath.Automations.Tests
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Commands;
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
            var update = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "UpdateArchitectureCustomerCommand -> ArchitectureCustomerResponse");
            var delete = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "DeleteArchitectureCustomerCommand -> void");
            var patch = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "PatchArchitectureCustomerCommand -> ArchitectureCustomerResponse");
            var getOne = Assert.Single(manifest.Components, component =>
                component.Kind == "spider.flow" &&
                component.DisplayName == "GetArchitectureCustomerQuery -> ArchitectureCustomerResponse");

            Assert.Contains(manifest.Relations, relation => relation.SourceId == create.Id && relation.TargetId == "turtlepath.command.create" && relation.Kind == "implements");
            Assert.Contains(manifest.Relations, relation => relation.SourceId == update.Id && relation.TargetId == "turtlepath.command.update" && relation.Kind == "implements");
            Assert.Contains(manifest.Relations, relation => relation.SourceId == delete.Id && relation.TargetId == "turtlepath.command.delete" && relation.Kind == "implements");
            Assert.Contains(manifest.Relations, relation => relation.SourceId == patch.Id && relation.TargetId == "turtlepath.command.patch" && relation.Kind == "implements");
            Assert.Contains(manifest.Components, component => component.Id == $"{create.Id}.response" && component.Metadata["tags"].Contains("reload"));
            Assert.Contains(manifest.Components, component => component.Id == $"{update.Id}.load");
            Assert.Contains(manifest.Components, component => component.Id == $"{delete.Id}.delete");
            Assert.Contains(manifest.Components, component => component.Id == $"{patch.Id}.patch");
            Assert.Contains(manifest.Components, component => component.Id == $"{getOne.Id}.filter");
        }

        private sealed class ArchitectureAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<ArchitectureCustomer>()
                    .ToCreate<CreateArchitectureCustomerCommand, ArchitectureCustomerResponse>(mutation => mutation
                        .ValidateRequest(false)
                        .Include(customer => customer.Parent))
                    .ToUpdate<UpdateArchitectureCustomerCommand, ArchitectureCustomerResponse>(mutation => mutation.ValidateRequest(true))
                    .ToDelete<DeleteArchitectureCustomerCommand>()
                    .ToPatch<PatchArchitectureCustomerCommand, ArchitectureCustomerResponse>()
                    .ToGetById<GetArchitectureCustomerByIdQuery, ArchitectureCustomerResponse>()
                    .ToGetOne<GetArchitectureCustomerQuery, ArchitectureCustomerResponse>()
                    .ToGetMany<GetArchitectureCustomersQuery, ArchitectureCustomerResponse>(query => query.DefaultSort("Name"))
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

        private sealed class UpdateArchitectureCustomerCommand : IBaseRequest<CId>, IRequest<ArchitectureCustomerResponse>
        {
            public CId Id { get; set; }
        }

        private sealed class DeleteArchitectureCustomerCommand : IBaseRequest<CId>, IRequest
        {
            public CId Id { get; set; }
        }

        private sealed class PatchArchitectureCustomerCommand : IBaseRequest<CId>, IRequest<ArchitectureCustomerResponse>, IPatchAction<ArchitectureCustomer>
        {
            public CId Id { get; set; }

            public ValueTask PatchAsync(ArchitectureCustomer entity, CancellationToken cancellationToken = default)
                => ValueTask.CompletedTask;
        }

        private sealed class GetArchitectureCustomerByIdQuery : IRequest<ArchitectureCustomerResponse>
        {
        }

        private sealed class GetArchitectureCustomerQuery : IRequest<ArchitectureCustomerResponse>
        {
        }

        private sealed class GetArchitectureCustomersQuery : IRequest<IEnumerable<ArchitectureCustomerResponse>>
        {
        }

        private sealed class SearchArchitectureCustomersQuery : IRequest<PagedResponse<ArchitectureCustomerResponse>>
        {
        }
    }
}
