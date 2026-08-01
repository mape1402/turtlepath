namespace TurtlePath.Automations.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using System.Reflection;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    public class AutomationHandlerRegistrationTests
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

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IRequestHandler<CreateCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericCreateCommandHandler<CreateCustomerCommand, CustomerResponse, Customer, CId>));
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

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IRequestHandler<DeleteCustomerCommand>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericDeleteCommandHandler<DeleteCustomerCommand, Customer, CId>));
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

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<GetCustomerByIdQuery, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericGetByIdQueryHandler<GetCustomerByIdQuery, Customer, CustomerResponse, CId>));
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

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<GetCustomerByEmailQuery, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericGetOneQueryHandler<GetCustomerByEmailQuery, CId, Customer, CustomerResponse, CId>));
            AssertOverrides(
                handler.ImplementationType,
                "GetFilterExpression",
                typeof(GetCustomerByEmailQuery));
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

            CreateRegistration().Register(services, [descriptor]);

            var registry = services
                .Select(service => service.ImplementationInstance)
                .OfType<AutomationDescriptorRegistry>()
                .Single();
            var registeredDescriptor = registry.Find(typeof(SearchCustomersQuery), typeof(PagedResponse<CustomerResponse>));

            Assert.NotNull(registeredDescriptor);
            Assert.Equal("Name", registeredDescriptor.DefaultSortProperty);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<SearchCustomersQuery, PagedResponse<CustomerResponse>>));

            Assert.NotNull(handler);
            AssertOverridesProperty(handler.ImplementationType!, "DefaultSorts");
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

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<PatchCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericPatchCommandHandler<PatchCustomerCommand, CustomerResponse, Customer, CId>));
            AssertOverrides(
                handler.ImplementationType,
                "BuildResponseAsync",
                typeof(PatchCustomerCommand),
                typeof(Customer),
                typeof(CancellationToken));
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

            var exception = Assert.Throws<NotSupportedException>(() => CreateRegistration().Register(services, [descriptor]));

            Assert.Contains(nameof(AutomationOperationKind.Patch), exception.Message);
            Assert.Contains(nameof(InvalidPatchCustomerCommand), exception.Message);
        }

        [Fact]
        public void Register_uses_configured_handler_type_generator()
        {
            var services = new ServiceCollection();
            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));
            var generator = new StubHandlerTypeGenerator(typeof(ConfiguredCreateCustomerHandler));

            new AutomationHandlerRegistration(generator).Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<CreateCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.Equal(typeof(ConfiguredCreateCustomerHandler), handler.ImplementationType);
            Assert.Same(descriptor, generator.Descriptor);
        }

        public sealed class Customer : BaseEntity
        {
        }

        public sealed class CustomerResponse : IBaseResponse<CId>
        {
            public CId Id { get; set; }
        }

        public sealed class CreateCustomerCommand : IRequest<CustomerResponse>
        {
        }

        public sealed class DeleteCustomerCommand : IRequest, TurtlePath.Models.Requests.IBaseRequest<CId>
        {
            public CId Id { get; set; }
        }

        public sealed class GetCustomerByIdQuery : GenericGetByIdQuery<Customer, CustomerResponse, CId>
        {
            public GetCustomerByIdQuery(CId id) : base(id)
            {
            }
        }

        public sealed class GetCustomerByEmailQuery : GenericGetOneQuery<CId, Customer, CustomerResponse, CId>
        {
        }

        public sealed class SearchCustomersQuery : GenericGetPagedInfoQuery<Customer, CustomerResponse, CId>
        {
            public SearchCustomersQuery(PagedSettings pagedSettings) : base(pagedSettings)
            {
            }
        }

        public sealed class PatchCustomerCommand : IRequest<CustomerResponse>, TurtlePath.Models.Requests.IBaseRequest<CId>, IPatchAction<Customer>
        {
            public CId Id { get; set; }

            public ValueTask PatchAsync(Customer entity, CancellationToken cancellationToken = default)
                => ValueTask.CompletedTask;
        }

        public sealed class InvalidPatchCustomerCommand : IRequest<CustomerResponse>, TurtlePath.Models.Requests.IBaseRequest<CId>
        {
            public CId Id { get; set; }
        }

        public sealed class ConfiguredCreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CustomerResponse>
        {
            public Task<CustomerResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken = default)
                => Task.FromResult(new CustomerResponse());
        }

        private static AutomationHandlerRegistration CreateRegistration()
            => new(new AutomationHandlerTypeGenerator(
                new DynaBee.FluentApi.DependencyInjection.DynaBeeAssemblyBuilderFactory(),
                new AutomationHandlerGenerationOptions(),
                new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy()));

        private static void AssertGeneratedHandler(Type implementationType, Type expectedBaseType)
        {
            Assert.StartsWith("Generated", implementationType.Name);
            Assert.Equal("TurtlePath.Automations.Generated", implementationType.Assembly.GetName().Name);
            Assert.Equal(expectedBaseType, implementationType.BaseType);
        }

        private static void AssertOverrides(Type implementationType, string name, params Type[] parameterTypes)
        {
            var method = implementationType.GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                parameterTypes,
                null);

            Assert.NotNull(method);
            Assert.Equal(implementationType, method.DeclaringType);
        }

        private static void AssertOverridesProperty(Type implementationType, string name)
        {
            var property = implementationType.GetProperty(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.Equal(implementationType, property.GetMethod!.DeclaringType);
        }

        private sealed class StubHandlerTypeGenerator : IAutomationHandlerTypeGenerator
        {
            private readonly Type implementationType;

            public StubHandlerTypeGenerator(Type implementationType)
            {
                this.implementationType = implementationType;
            }

            public AutomationDescriptor Descriptor { get; private set; } = null!;

            public AutomationHandlerGenerationResult Generate(IReadOnlyCollection<AutomationDescriptor> descriptors)
            {
                Descriptor = descriptors.Single();

                return new AutomationHandlerGenerationResult(
                    [new AutomationGeneratedHandler(Descriptor, "ConfiguredCreateCustomerHandler", implementationType)]);
            }
        }
    }
}
