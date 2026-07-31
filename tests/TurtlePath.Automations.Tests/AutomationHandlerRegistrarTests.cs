namespace TurtlePath.Automations.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
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
    }
}
