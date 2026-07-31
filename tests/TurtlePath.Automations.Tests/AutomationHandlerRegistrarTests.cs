namespace TurtlePath.Automations.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    public class AutomationHandlerRegistrarTests
    {
        [Fact]
        public void Register_adds_closed_create_handler_for_pelican_request()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var handler = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IRequestHandler<CreateCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
        }

        [Fact]
        public void Register_adds_closed_no_response_delete_handler_for_pelican_request()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Delete,
                typeof(DeleteCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.None);

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var handler = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IRequestHandler<DeleteCustomerCommand>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
        }

        [Fact]
        public void Register_adds_closed_get_by_id_query_handler_for_pelican_request()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetById,
                typeof(GetCustomerByIdQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<GetCustomerByIdQuery, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
        }

        [Fact]
        public void Register_adds_closed_get_one_query_handler_for_pelican_request()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetOne,
                typeof(GetCustomerByEmailQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<GetCustomerByEmailQuery, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
        }

        [Fact]
        public void Register_preserves_descriptor_customizations_for_generated_handlers()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetPaged,
                typeof(SearchCustomersQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(PagedResponse<CustomerResponse>),
                defaultSortProperty: "Name");

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var registry = services
                .Select(service => service.ImplementationInstance)
                .OfType<AutomationDescriptorRegistry>()
                .Single();
            var registeredDescriptor = registry.Find(typeof(SearchCustomersQuery), typeof(PagedResponse<CustomerResponse>));

            Assert.NotNull(registeredDescriptor);
            Assert.Equal("Name", registeredDescriptor.DefaultSortProperty);
        }

        [Fact]
        public void Register_adds_closed_patch_handler_when_request_implements_patch_action()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Patch,
                typeof(PatchCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            AutomationHandlerRegistrar.Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<PatchCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
        }

        [Fact]
        public void Register_rejects_patch_handler_when_request_does_not_implement_patch_action()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Patch,
                typeof(InvalidPatchCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            var exception = Assert.Throws<NotSupportedException>(() => AutomationHandlerRegistrar.Register(services, [descriptor]));

            Assert.Contains(nameof(AutomationOperationKind.Patch), exception.Message);
            Assert.Contains(nameof(InvalidPatchCustomerCommand), exception.Message);
        }

        private sealed class Customer : BaseEntity
        {
        }

        private sealed class CustomerResponse : IBaseResponse<CId>
        {
            public CId Id { get; set; }
        }

        private sealed class CreateCustomerCommand : IRequest<CustomerResponse>
        {
        }

        private sealed class DeleteCustomerCommand : IRequest, TurtlePath.Models.Requests.IBaseRequest<CId>
        {
            public CId Id { get; set; }
        }

        private sealed class GetCustomerByIdQuery : GenericGetByIdQuery<Customer, CustomerResponse, CId>
        {
            public GetCustomerByIdQuery(CId id) : base(id)
            {
            }
        }

        private sealed class GetCustomerByEmailQuery : GenericGetOneQuery<CId, Customer, CustomerResponse, CId>
        {
        }

        private sealed class SearchCustomersQuery : GenericGetPagedInfoQuery<Customer, CustomerResponse, CId>
        {
            public SearchCustomersQuery(PagedSettings pagedSettings) : base(pagedSettings)
            {
            }
        }

        private sealed class PatchCustomerCommand : IRequest<CustomerResponse>, TurtlePath.Models.Requests.IBaseRequest<CId>, IPatchAction<Customer>
        {
            public CId Id { get; set; }

            public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
                => ValueTask.CompletedTask;
        }

        private sealed class InvalidPatchCustomerCommand : IRequest<CustomerResponse>, TurtlePath.Models.Requests.IBaseRequest<CId>
        {
            public CId Id { get; set; }
        }
    }
}
