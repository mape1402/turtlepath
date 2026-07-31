namespace TurtlePath.Automations.Tests.Profiles
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Profiles;
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
                },
                descriptor =>
                {
                    Assert.Equal(AutomationOperationKind.GetById, descriptor.OperationKind);
                    Assert.Equal(typeof(GetCustomerByIdQuery), descriptor.RequestType);
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

        private sealed class CustomerAutomationProfile : TurtlePathAutomationProfile
        {
            public override void Configure(ITurtlePathAutomationBuilder builder)
            {
                builder.For<Customer>()
                    .ToCreate<CreateCustomerCommand, CustomerResponse>()
                    .ToUpdate<UpdateCustomerCommand, CustomerResponse>()
                    .ToDelete<DeleteCustomerCommand>()
                    .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
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

        private sealed class Customer : BaseEntity
        {
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

        private sealed class ImportCustomerCommand : IRequest<CustomerResponse>
        {
        }

        private sealed class UpdateCustomerCommand : IBaseRequest<CId>, IRequest<CustomerResponse>
        {
            public CId Id { get; set; }
        }

        private sealed class DeleteCustomerCommand : IBaseRequest<CId>, IRequest
        {
            public CId Id { get; set; }
        }

        private sealed class GetCustomerByIdQuery : IRequest<CustomerResponse>
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
    }
}
