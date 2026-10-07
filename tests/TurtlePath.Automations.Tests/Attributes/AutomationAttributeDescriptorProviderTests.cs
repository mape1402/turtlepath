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
            Assert.False(create.ValidateRequest);
            Assert.True(create.ReloadBeforeResponse);

            var delete = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(DeleteAttributedCustomerCommand));
            Assert.Equal(AutomationReturnMode.None, delete.ReturnMode);
            Assert.Null(delete.ResponseType);
            Assert.True(delete.ValidateRequest);
        }

        [Fact]
        public void GetDescriptors_returns_empty_when_no_assemblies_are_provided()
        {
            Assert.Empty(AutomationAttributeDescriptorProvider.GetDescriptors());
            Assert.Empty(AutomationAttributeDescriptorProvider.GetDescriptors(null));
        }

        [Fact]
        public void GetDescriptors_wraps_many_and_paged_response_types()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var many = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(GetManyAttributedCustomersQuery));
            Assert.Equal(typeof(IEnumerable<AttributedCustomerResponse>), many.ResponseType);
            Assert.Equal("Name", many.DefaultSortProperty);

            var paged = Assert.Single(descriptors, descriptor => descriptor.RequestType == typeof(GetPagedAttributedCustomersQuery));
            Assert.Equal(typeof(PagedResponse<AttributedCustomerResponse>), paged.ResponseType);
            Assert.Equal("-CreatedAt", paged.DefaultSortProperty);
        }

        [Fact]
        public void GetDescriptors_supports_custom_key_entities()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var descriptor = Assert.Single(descriptors, x => x.RequestType == typeof(UpdateAttributedLegacyCustomerCommand));
            Assert.Equal(typeof(int), descriptor.KeyType);
            Assert.Equal(typeof(AttributedLegacyCustomer), descriptor.EntityType);
        }

        [Fact]
        public void GetDescriptors_supports_validation_and_projection_attributes()
        {
            var descriptors = AutomationAttributeDescriptorProvider.GetDescriptors(typeof(AutomationAttributeDescriptorProviderTests).Assembly);

            var descriptor = Assert.Single(descriptors, x => x.RequestType == typeof(ProjectedAttributedCustomerCommand));

            Assert.False(descriptor.ValidateRequest);
            Assert.True(descriptor.ReloadBeforeResponse);
        }

        [Fact]
        public void Automation_attributes_expose_constructor_options()
        {
            var create = new CreateAutomationAttribute(typeof(AttributedCustomer));
            var createWithResponse = new CreateAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));
            var update = new UpdateAutomationAttribute(typeof(AttributedCustomer));
            var updateWithResponse = new UpdateAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));
            var delete = new DeleteAutomationAttribute(typeof(AttributedCustomer));
            var deleteWithResponse = new DeleteAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));
            var patch = new PatchAutomationAttribute(typeof(AttributedCustomer));
            var patchWithResponse = new PatchAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));
            var getById = new GetByIdAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));
            var getOne = new GetOneAutomationAttribute(typeof(AttributedCustomer), typeof(AttributedCustomerResponse));

            Assert.True(create.ValidateRequest);
            Assert.Equal(typeof(AttributedCustomer), create.EntityType);
            Assert.Null(create.ResponseType);
            Assert.Equal(typeof(AttributedCustomerResponse), createWithResponse.ResponseType);
            Assert.True(update.ValidateRequest);
            Assert.Equal(typeof(AttributedCustomerResponse), updateWithResponse.ResponseType);
            Assert.False(delete.ValidateRequest);
            Assert.Equal(typeof(AttributedCustomerResponse), deleteWithResponse.ResponseType);
            Assert.False(patch.ValidateRequest);
            Assert.Equal(typeof(AttributedCustomerResponse), patchWithResponse.ResponseType);
            Assert.Equal(typeof(AttributedCustomerResponse), getById.ResponseType);
            Assert.Equal(typeof(AttributedCustomerResponse), getOne.ResponseType);
            Assert.Equal(AutomationOperationKind.Patch, patch.OperationKind);
            Assert.Equal(AutomationOperationKind.GetById, getById.OperationKind);
            Assert.Equal(AutomationOperationKind.GetOne, getOne.OperationKind);
            Assert.Throws<ArgumentNullException>(() => new GetOneAutomationAttribute(null, typeof(AttributedCustomerResponse)));
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

        [CreateAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse), ValidateRequest = false, ReloadBeforeResponse = true)]
        private sealed class CreateAttributedCustomerCommand : IRequest<AttributedCustomerResponse>
        {
        }

        [DeleteAutomation(typeof(AttributedCustomer), ValidateRequest = true)]
        private sealed class DeleteAttributedCustomerCommand : IRequest
        {
        }

        [GetManyAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse), DefaultSort = "Name")]
        private sealed class GetManyAttributedCustomersQuery : IRequest<IEnumerable<AttributedCustomerResponse>>
        {
        }

        [GetPagedAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse), DefaultSort = "-CreatedAt")]
        private sealed class GetPagedAttributedCustomersQuery : IRequest<PagedResponse<AttributedCustomerResponse>>
        {
        }

        [UpdateAutomation(typeof(AttributedLegacyCustomer), typeof(AttributedLegacyCustomerResponse))]
        private sealed class UpdateAttributedLegacyCustomerCommand : IRequest<AttributedLegacyCustomerResponse>
        {
        }

        [AutomationValidation(false)]
        [AutomationProjection(true)]
        [UpdateAutomation(typeof(AttributedCustomer), typeof(AttributedCustomerResponse), ValidateRequest = true)]
        private sealed class ProjectedAttributedCustomerCommand : IRequest<AttributedCustomerResponse>
        {
        }
    }
}
