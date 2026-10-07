namespace TurtlePath.Automations.Tests.Profiles
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Profiles;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    public class AutomationProfileDescriptorBuilderTests
    {
        [Fact]
        public void Build_creates_descriptors_from_recommended_entity_profile()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new CustomerAutomationProfile());

            Assert.Collection(
                descriptors.OrderBy(x => x.OperationKind),
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.Create, descriptor.OperationKind);
                    Assert.Equal(typeof(Customer), descriptor.EntityType);
                    Assert.Equal(typeof(CId), descriptor.KeyType);
                    Assert.Equal(typeof(CreateCustomerCommand), descriptor.RequestType);
                    Assert.Equal(typeof(CustomerResponse), descriptor.ResponseType);
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.Update, descriptor.OperationKind);
                    Assert.Equal(typeof(UpdateCustomerCommand), descriptor.RequestType);
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.Delete, descriptor.OperationKind);
                    Assert.Equal(typeof(DeleteCustomerCommand), descriptor.RequestType);
                    Assert.Equal(AutomationReturnMode.None, descriptor.ReturnMode);
                    Assert.True(descriptor.ValidateRequest);
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.GetById, descriptor.OperationKind);
                    Assert.Equal(typeof(GetCustomerByIdQuery), descriptor.RequestType);
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.GetMany, descriptor.OperationKind);
                    Assert.Equal(typeof(GetCustomersQuery), descriptor.RequestType);
                    Assert.Equal(typeof(IEnumerable<CustomerResponse>), descriptor.ResponseType);
                    Assert.Equal("Name", descriptor.DefaultSortProperty);
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.GetPaged, descriptor.OperationKind);
                    Assert.Equal(typeof(SearchCustomersQuery), descriptor.RequestType);
                    Assert.Equal(typeof(PagedResponse<CustomerResponse>), descriptor.ResponseType);
                    Assert.Equal("Name", descriptor.DefaultSortProperty);
                });
        }

        [Fact]
        public void Build_supports_custom_key_entities()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new LegacyAutomationProfile());

            var descriptor = Assert.Single(descriptors);
            Assert.Equal(typeof(LegacyCustomer), descriptor.EntityType);
            Assert.Equal(typeof(int), descriptor.KeyType);
            Assert.NotNull(descriptor.KeySelector);
            Assert.Equal("Legacy customer not found.", descriptor.NotFoundMessage);
        }

        [Fact]
        public void Build_allows_repeated_mutations_with_different_models()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new RepeatedCreateAutomationProfile());

            Assert.Equal(2, descriptors.Count);
            Assert.Contains(descriptors, x => x.RequestType == typeof(CreateCustomerCommand));
            Assert.Contains(descriptors, x => x.RequestType == typeof(ImportCustomerCommand));
        }

        [Fact]
        public void Build_preserves_mutation_response_projection_options()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new ResponseProjectionAutomationProfile());

            var descriptor = Assert.Single(descriptors);

            Assert.True(descriptor.ReloadBeforeResponse);
            Assert.False(descriptor.ValidateRequest);
            var include = Assert.Single(descriptor.ResponseIncludeExpressions);
            Assert.Equal("customer.Parent", include.Body.ToString());
        }

        [Fact]
        public void Build_supports_validation_projection_aliases_handler_and_metadata()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new ExtendedMutationAutomationProfile());

            var descriptor = Assert.Single(descriptors);

            Assert.True(descriptor.ValidateRequest);
            Assert.True(descriptor.ReloadBeforeResponse);
            Assert.Equal(typeof(CustomCreateCustomerCommandHandler), descriptor.HandlerType);
            var metadata = Assert.IsType<TestAutomationMetadata>(descriptor.Metadata["test.metadata"]);
            Assert.Equal("enabled", metadata.Value);
        }

        [Fact]
        public void Build_supports_query_handler_and_metadata()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new ExtendedQueryAutomationProfile());

            var descriptor = Assert.Single(descriptors);

            Assert.Equal(typeof(CustomGetCustomerByIdQueryHandler), descriptor.HandlerType);
            var metadata = Assert.IsType<TestAutomationMetadata>(descriptor.Metadata["test.metadata"]);
            Assert.Equal("query", metadata.Value);
        }

        [Fact]
        public void Build_allows_disabling_response_projection_after_include()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new DisabledResponseProjectionAutomationProfile());

            var descriptor = Assert.Single(descriptors);

            Assert.False(descriptor.ReloadBeforeResponse);
            var include = Assert.Single(descriptor.ResponseIncludeExpressions);
            Assert.Equal("customer.Parent", include.Body.ToString());
        }

        [Fact]
        public void Build_supports_all_mutation_and_query_builder_variants()
        {
            var descriptors = AutomationProfileDescriptorBuilder.Build(new VariantAutomationProfile());

            Assert.Contains(descriptors, descriptor =>
                descriptor.OperationKind == AutomationOperationKind.Create &&
                descriptor.RequestType == typeof(CreateCustomerWithoutResponseCommand) &&
                descriptor.ReturnMode == AutomationReturnMode.None);
            Assert.Contains(descriptors, descriptor =>
                descriptor.OperationKind == AutomationOperationKind.Update &&
                descriptor.RequestType == typeof(UpdateCustomerWithoutResponseCommand) &&
                descriptor.ReturnMode == AutomationReturnMode.None);
            Assert.Contains(descriptors, descriptor =>
                descriptor.OperationKind == AutomationOperationKind.Delete &&
                descriptor.RequestType == typeof(DeleteCustomerWithResponseCommand) &&
                descriptor.ResponseType == typeof(CustomerResponse));
            Assert.Contains(descriptors, descriptor =>
                descriptor.OperationKind == AutomationOperationKind.Patch &&
                descriptor.RequestType == typeof(PatchCustomerCommand) &&
                descriptor.ResponseType == typeof(CustomerResponse));
            Assert.Contains(descriptors, descriptor =>
                descriptor.OperationKind == AutomationOperationKind.Patch &&
                descriptor.RequestType == typeof(PatchCustomerWithoutResponseCommand) &&
                descriptor.ReturnMode == AutomationReturnMode.None);

            var getOne = Assert.Single(descriptors, descriptor => descriptor.OperationKind == AutomationOperationKind.GetOne);
            Assert.Equal(typeof(GetCustomerByNameQuery), getOne.RequestType);
            Assert.NotNull(getOne.KeySelector);
            Assert.Equal("-Name", getOne.DefaultSortProperty);
            Assert.Equal("Customer not found.", getOne.NotFoundMessage);
        }

        [Fact]
        public void Query_builder_validates_key_selector()
        {
            var builder = new QueryAutomationBuilder<GetCustomerByNameQuery, Customer, CId>();

            var descriptors = Assert.Throws<ArgumentNullException>(() => builder.GetKeyFrom(null));

            Assert.Equal("keySelector", descriptors.ParamName);
        }

        private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>()
                    .ToUpdate<UpdateCustomerCommand, CustomerResponse>()
                    .ToDelete<DeleteCustomerCommand>(mutation => mutation.ValidateRequest())
                    .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
                    .ToGetMany<GetCustomersQuery, CustomerResponse>(query => query.DefaultSort("Name"))
                    .ToGetPaged<SearchCustomersQuery, CustomerResponse>(query => query.DefaultSort("Name"));
            }
        }

        private sealed class LegacyAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<LegacyCustomer, int>()
                    .ToUpdate<UpdateLegacyCustomerCommand, LegacyCustomerResponse>(mutation => mutation
                        .GetKeyFrom(command => command.LegacyId)
                        .NotFoundMessage("Legacy customer not found."));
            }
        }

        private sealed class RepeatedCreateAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>()
                    .ToCreate<ImportCustomerCommand, CustomerResponse>();
            }
        }

        private sealed class ResponseProjectionAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>(mutation => mutation
                        .ValidateRequest(false)
                        .Include(customer => customer.Parent));
            }
        }

        private sealed class DisabledResponseProjectionAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>(mutation => mutation
                        .Include(customer => customer.Parent)
                        .ReloadBeforeResponse(false));
            }
        }

        private sealed class ExtendedMutationAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>(mutation =>
                    {
                        mutation
                            .Validate()
                            .Projection()
                            .UseHandler<CustomCreateCustomerCommandHandler>();
                        mutation.SetMetadata("test.metadata", new TestAutomationMetadata("enabled"));
                    });
            }
        }

        private sealed class ExtendedQueryAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToGetById<GetCustomerByIdQuery, CustomerResponse>(query =>
                    {
                        query.UseHandler<CustomGetCustomerByIdQueryHandler>();
                        query.SetMetadata("test.metadata", new TestAutomationMetadata("query"));
                    });
            }
        }

        private sealed class VariantAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerWithoutResponseCommand>()
                    .ToUpdate<UpdateCustomerWithoutResponseCommand>()
                    .ToDelete<DeleteCustomerWithResponseCommand, CustomerResponse>()
                    .ToPatch<PatchCustomerCommand, CustomerResponse>()
                    .ToPatch<PatchCustomerWithoutResponseCommand>()
                    .ToGetOne<GetCustomerByNameQuery, CustomerResponse>(query => query
                        .GetKeyFrom(request => request.Id)
                        .DefaultSort("-Name")
                        .NotFoundMessage("Customer not found."));
            }
        }

        private sealed class Customer : BaseEntity
        {
            public Customer Parent { get; set; }
        }

        private sealed class LegacyCustomer : IEntity<int>
        {
            public int Id { get; set; }
        }

        private sealed class CustomerResponse : IBaseResponse<CId>
        {
            public CId Id { get; set; }
        }

        private sealed class LegacyCustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }
        }

        private sealed class CreateCustomerCommand : IRequest<CustomerResponse>
        {
        }

        private sealed class CreateCustomerWithoutResponseCommand : IRequest
        {
        }

        private sealed class ImportCustomerCommand : IRequest<CustomerResponse>
        {
        }

        private sealed class UpdateCustomerCommand : IBaseRequest<CId>, IRequest<CustomerResponse>
        {
            public CId Id { get; set; }
        }

        private sealed class UpdateCustomerWithoutResponseCommand : IBaseRequest<CId>, IRequest
        {
            public CId Id { get; set; }
        }

        private sealed class DeleteCustomerCommand : IBaseRequest<CId>, IRequest
        {
            public CId Id { get; set; }
        }

        private sealed class DeleteCustomerWithResponseCommand : IBaseRequest<CId>, IRequest<CustomerResponse>
        {
            public CId Id { get; set; }
        }

        private sealed class PatchCustomerCommand : IBaseRequest<CId>, IRequest<CustomerResponse>, IPatchAction<Customer>
        {
            public CId Id { get; set; }

            public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
                => ValueTask.CompletedTask;
        }

        private sealed class PatchCustomerWithoutResponseCommand : IBaseRequest<CId>, IRequest, IPatchAction<Customer>
        {
            public CId Id { get; set; }

            public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
                => ValueTask.CompletedTask;
        }

        private sealed class GetCustomerByIdQuery : IRequest<CustomerResponse>
        {
        }

        private sealed class GetCustomerByNameQuery : IRequest<CustomerResponse>
        {
            public CId Id { get; set; }
        }

        private sealed class GetCustomersQuery : IRequest<IEnumerable<CustomerResponse>>
        {
        }

        private sealed class SearchCustomersQuery : IRequest<PagedResponse<CustomerResponse>>
        {
        }

        private sealed class UpdateLegacyCustomerCommand : IBaseRequest<int>, IRequest<LegacyCustomerResponse>
        {
            public int Id { get; set; }

            public int LegacyId { get; set; }
        }

        private sealed record TestAutomationMetadata(string Value);

        private sealed class CustomCreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerResponse>
        {
            public Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken = default)
                => Task.FromResult(new CustomerResponse());
        }

        private sealed class CustomGetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerResponse>
        {
            public Task<CustomerResponse> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken = default)
                => Task.FromResult(new CustomerResponse());
        }
    }
}
