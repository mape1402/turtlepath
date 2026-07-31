namespace TurtlePath.Automations.Tests.Attributes
{
    using Pelican.Mediator;
    using TurtlePath.Automations.Attributes;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Responses;

    public class AutomationAttributeDescriptorProviderTests
    {
        [Fact]
        public void GetDescriptors_creates_descriptors_from_attributes()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var create = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(CreateAttributedCustomerCommand));
            Assert.Equal(AutomationOperationKind.Create, create.OperationKind);
            Assert.Equal(typeof(AttributedCustomer), create.EntityType);
            Assert.Equal(typeof(CId), create.KeyType);
            Assert.Equal(typeof(AttributedCustomerResponse), create.ResponseType);
            Assert.Equal(AutomationSourceKind.Attribute, create.SourceKind);

            var delete = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(DeleteAttributedCustomerCommand));
            Assert.Equal(AutomationReturnMode.None, delete.ReturnMode);
            Assert.Null(delete.ResponseType);
        }

        [Fact]
        public void GetDescriptors_wraps_many_and_paged_response_types()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var many = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(GetManyAttributedCustomersQuery));
            Assert.Equal(typeof(IEnumerable<AttributedCustomerResponse>), many.ResponseType);

            var paged = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(GetPagedAttributedCustomersQuery));
            Assert.Equal(typeof(PagedResponse<AttributedCustomerResponse>), paged.ResponseType);
        }

        [Fact]
        public void GetDescriptors_supports_custom_key_entities()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var descriptor = Assert.Single(descriptors, x => x.RequestType == typeof(UpdateAttributedLegacyCustomerCommand));
            Assert.Equal(typeof(int), descriptor.KeyType);
            Assert.Equal(typeof(AttributedLegacyCustomer), descriptor.EntityType);
        }

        private sealed class AttributedCustomer : BaseEntity
        {
        }

        private sealed class AttributedLegacyCustomer : IEntity<int>
        {
            public int Id { get; set; }
        }

        private sealed class AttributedCustomerResponse : IBaseResponse<CId>
        {
            public CId Id { get; set; }
        }

        private sealed class AttributedLegacyCustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }
        }

        [CreateAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse))]
        private sealed class CreateAttributedCustomerCommand : IRequest<AttributedCustomerResponse>
        {
        }

        [DeleteAutomation(typeof(AttributedCustomer))]
        private sealed class DeleteAttributedCustomerCommand : IRequest
        {
        }

        [GetManyAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse))]
        private sealed class GetManyAttributedCustomersQuery : IRequest<IEnumerable<AttributedCustomerResponse>>
        {
        }

        [GetPagedAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse))]
        private sealed class GetPagedAttributedCustomersQuery : IRequest<PagedResponse<AttributedCustomerResponse>>
        {
        }

        [UpdateAutomation(typeof(AttributedLegacyCustomer), typeof(AttributedLegacyCustomerResponse))]
        private sealed class UpdateAttributedLegacyCustomerCommand : IRequest<AttributedLegacyCustomerResponse>
        {
        }
    }
}
