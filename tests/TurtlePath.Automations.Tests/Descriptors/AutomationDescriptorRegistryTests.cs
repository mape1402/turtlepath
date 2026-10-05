namespace TurtlePath.Automations.Tests.Descriptors
{
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;

    public class AutomationDescriptorRegistryTests
    {
        [Fact]
        public void Add_keeps_single_descriptor_for_equivalent_registration()
        {
            var registry = new AutomationDescriptorRegistry();
            var descriptor = CreateCustomerCreateDescriptor();

            registry.Add(descriptor);
            registry.Add(CreateCustomerCreateDescriptor());

            Assert.Single(registry.Descriptors);
        }

        [Fact]
        public void Add_throws_when_same_request_response_points_to_different_operation()
        {
            var registry = new AutomationDescriptorRegistry();
            registry.Add(CreateCustomerCreateDescriptor());

            var candidate = new AutomationDescriptor(
                AutomationOperationKind.Update,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            Assert.Throws<AutomationDescriptorConflictException>(() => registry.Add(candidate));
        }

        [Fact]
        public void Add_replaces_attribute_descriptor_with_profile_descriptor()
        {
            var registry = new AutomationDescriptorRegistry();
            registry.Add(new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(LegacyCustomer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse),
                AutomationSourceKind.Attribute));

            registry.Add(CreateCustomerCreateDescriptor());

            var descriptor = Assert.Single(registry.Descriptors);
            Assert.Equal(typeof(Customer), descriptor.EntityType);
            Assert.Equal(AutomationSourceKind.Profile, descriptor.SourceKind);
        }

        [Fact]
        public void Add_ignores_attribute_descriptor_when_profile_already_exists()
        {
            var registry = new AutomationDescriptorRegistry();
            registry.Add(CreateCustomerCreateDescriptor());

            registry.Add(new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(LegacyCustomer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse),
                AutomationSourceKind.Attribute));

            var descriptor = Assert.Single(registry.Descriptors);
            Assert.Equal(typeof(Customer), descriptor.EntityType);
        }

        [Fact]
        public void Add_rejects_entity_that_does_not_match_key_contract()
        {
            var registry = new AutomationDescriptorRegistry();
            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(int),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            Assert.Throws<ArgumentException>(() => registry.Add(descriptor));
        }

        [Fact]
        public void Add_rejects_query_without_response()
        {
            var registry = new AutomationDescriptorRegistry();
            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetById,
                typeof(GetCustomerByIdQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.None);

            Assert.Throws<ArgumentException>(() => registry.Add(descriptor));
        }

        [Fact]
        public void Registry_and_conflict_exception_validate_arguments()
        {
            var registry = new AutomationDescriptorRegistry();
            var descriptor = CreateCustomerCreateDescriptor();

            registry.Add(descriptor);

            Assert.Throws<ArgumentNullException>(() => new AutomationDescriptorRegistry(null));
            Assert.Throws<ArgumentNullException>(() => registry.Add(null));
            Assert.Throws<ArgumentNullException>(() => registry.Find(null, typeof(CustomerResponse)));
            Assert.Throws<ArgumentNullException>(() => registry.Find(typeof(CreateCustomerCommand), null));
            Assert.Null(registry.Find(typeof(GetCustomerByIdQuery), typeof(CustomerResponse)));
            Assert.Throws<ArgumentNullException>(() => AutomationDescriptorValidator.Validate(null));
            Assert.Throws<ArgumentNullException>(() => new AutomationDescriptorConflictException(null, descriptor));
            Assert.Throws<ArgumentNullException>(() => new AutomationDescriptorConflictException(descriptor, null));
            var conflict = new AutomationDescriptorConflictException(descriptor, CreateCustomerCreateDescriptor());

            Assert.Same(descriptor, conflict.Current);
            Assert.NotNull(conflict.Candidate);
            Assert.False(descriptor.IsEquivalentTo(null));

            var key = new AutomationDescriptorKey(typeof(CreateCustomerCommand), AutomationReturnMode.Response, typeof(CustomerResponse));

            Assert.Equal(typeof(CreateCustomerCommand), key.RequestType);
            Assert.Equal(AutomationReturnMode.Response, key.ReturnMode);
            Assert.Equal(typeof(CustomerResponse), key.ResponseType);
        }

        [Fact]
        public void Add_rejects_invalid_response_mode_combinations()
        {
            var registry = new AutomationDescriptorRegistry();

            Assert.Throws<ArgumentException>(() => registry.Add(new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response)));
            Assert.Throws<ArgumentException>(() => registry.Add(new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.None,
                typeof(CustomerResponse))));
        }

        private static AutomationDescriptor CreateCustomerCreateDescriptor()
            => new(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

        private sealed class Customer : BaseEntity
        {
        }

        private sealed class LegacyCustomer : IEntity<int>
        {
            public int Id { get; set; }
        }

        private sealed class CreateCustomerCommand
        {
        }

        private sealed class GetCustomerByIdQuery
        {
        }

        private sealed class CustomerResponse
        {
        }
    }
}
