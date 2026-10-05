namespace TurtlePath.Automations.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using System.Reflection;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Automations.Generation.DynaBeeIntegration;
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
        public void Register_adds_closed_delete_handler_with_response_projection_override()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Delete,
                typeof(DeleteCustomerWithResponseCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<DeleteCustomerWithResponseCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.NotNull(handler.ImplementationType);
            AssertGeneratedHandler(
                handler.ImplementationType,
                typeof(GenericDeleteCommandHandler<DeleteCustomerWithResponseCommand, CustomerResponse, Customer, CId>));
            AssertOverrides(
                handler.ImplementationType,
                "BuildResponseAsync",
                typeof(DeleteCustomerWithResponseCommand),
                typeof(Customer),
                typeof(CancellationToken));
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
        public void Register_preserves_get_many_default_sort_for_generated_handlers()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetMany,
                typeof(GetCustomersQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(IEnumerable<CustomerResponse>),
                defaultSortProperty: "-Name");

            CreateRegistration().Register(services, [descriptor]);

            var registry = services
                .Select(service => service.ImplementationInstance)
                .OfType<AutomationDescriptorRegistry>()
                .Single();
            var registeredDescriptor = registry.Find(typeof(GetCustomersQuery), typeof(IEnumerable<CustomerResponse>));

            Assert.NotNull(registeredDescriptor);
            Assert.Equal("-Name", registeredDescriptor.DefaultSortProperty);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<GetCustomersQuery, IEnumerable<CustomerResponse>>));

            Assert.NotNull(handler);
            AssertOverridesProperty(handler.ImplementationType!, "DefaultSorts");
        }

        [Fact]
        public void Register_overrides_validation_when_descriptor_configures_it()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Delete,
                typeof(DeleteCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.None,
                validateRequest: true);

            CreateRegistration().Register(services, [descriptor]);

            var handler = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IRequestHandler<DeleteCustomerCommand>));

            Assert.NotNull(handler);
            AssertOverridesProperty(handler.ImplementationType!, "ValidateRequest");
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
        }

        [Fact]
        public void Register_adds_command_response_options_for_mutation_response_projection()
        {
            var services = new ServiceCollection();
            services.AddTurtlePath();

            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.Create,
                typeof(CreateCustomerCommand),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse),
                reloadBeforeResponse: true,
                responseIncludeExpressions: [Expression((Customer customer) => customer.Parent)]);

            CreateRegistration().Register(services, [descriptor]);

            var service = services.Single(service =>
                service.ServiceType == typeof(ICommandResponseOptions<CreateCustomerCommand, Customer>));
            var options = Assert.IsType<DescriptorCommandResponseOptionsProxy>(
                new DescriptorCommandResponseOptionsProxy(service.ImplementationFactory!(null)));

            Assert.True(options.Value.UseProjectionFromStorage);
            var include = Assert.Single(options.Value.GetIncludeExpressions(new CreateCustomerCommand()));
            Assert.Equal("customer.Parent", include.Body.ToString());
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

            new AutomationHandlerRegistration(
                generator,
                new AutomationHandlerServiceTypeResolver(),
                new Options.AutomationQueryOptionsRegistration(),
                new Options.AutomationCommandResponseOptionsRegistration()).Register(services, [descriptor]);

            var handler = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IRequestHandler<CreateCustomerCommand, CustomerResponse>));

            Assert.NotNull(handler);
            Assert.Equal(typeof(ConfiguredCreateCustomerHandler), handler.ImplementationType);
            Assert.Same(descriptor, generator.Descriptor);
        }

        [Fact]
        public void DynaBee_generator_validates_arguments_and_base_type_contracts()
        {
            var descriptor = new AutomationDescriptor(
                AutomationOperationKind.GetOne,
                typeof(GetCustomerByEmailQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(CustomerResponse));
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => new DynaBeeAutomationHandlerTypeGenerator(
                null!,
                new AutomationHandlerGenerationOptions(),
                new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy()));
            Assert.Throws<ArgumentNullException>(() => new DynaBeeAutomationHandlerTypeGenerator(
                new DynaBee.FluentApi.DependencyInjection.DynaBeeAssemblyBuilderFactory(),
                null!,
                new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy()));
            Assert.Throws<ArgumentNullException>(() => new DynaBeeAutomationHandlerTypeGenerator(
                new DynaBee.FluentApi.DependencyInjection.DynaBeeAssemblyBuilderFactory(),
                new AutomationHandlerGenerationOptions(),
                null!,
                new DefaultAutomationHandlerTypeNamePolicy()));
            Assert.Throws<ArgumentNullException>(() => new DynaBeeAutomationHandlerTypeGenerator(
                new DynaBee.FluentApi.DependencyInjection.DynaBeeAssemblyBuilderFactory(),
                new AutomationHandlerGenerationOptions(),
                new AutomationHandlerBaseTypeResolver(),
                null!));
            Assert.Throws<ArgumentNullException>(() => CreateGenerator().Generate(null!));
            Assert.Throws<ArgumentNullException>(() => new DynaBeeAutomationHandlerTypeGeneratorFactory().Create(null!));
            Assert.NotNull(new DynaBeeAutomationHandlerTypeGeneratorFactory().Create(services));
            Assert.Throws<ArgumentNullException>(() => new AutomationHandlerGenerationResult(null!));
            Assert.Throws<ArgumentNullException>(() => new AutomationHandlerGenerationResult([]).Find(null!));
            Assert.Throws<ArgumentNullException>(() => new AutomationHandlerServiceTypeResolver().Resolve(null!));
            Assert.Throws<ArgumentNullException>(() => new DefaultAutomationHandlerTypeNamePolicy().CreateName(null!, 1));
            Assert.Throws<ArgumentNullException>(() => new AutomationGeneratedHandler(null!, "Handler", typeof(ConfiguredCreateCustomerHandler)));
            Assert.Throws<ArgumentException>(() => new AutomationGeneratedHandler(descriptor, " ", typeof(ConfiguredCreateCustomerHandler)));
            Assert.Throws<ArgumentNullException>(() => new AutomationGeneratedHandler(descriptor, "Handler", null!));
            Assert.Contains("constructor", Assert.Throws<InvalidOperationException>(() =>
                CreateGenerator(new StubBaseTypeResolver(typeof(MissingServiceProviderConstructorBase))).Generate([descriptor])).Message);
            Assert.Contains("GetFilterExpression", Assert.Throws<InvalidOperationException>(() =>
                CreateGenerator(new StubBaseTypeResolver(typeof(MissingVirtualMethodBase))).Generate([descriptor])).Message);

            var pagedDescriptor = new AutomationDescriptor(
                AutomationOperationKind.GetPaged,
                typeof(SearchCustomersQuery),
                typeof(Customer),
                typeof(CId),
                AutomationReturnMode.Response,
                typeof(PagedResponse<CustomerResponse>));

            Assert.Contains("DefaultSorts", Assert.Throws<InvalidOperationException>(() =>
                CreateGenerator(new StubBaseTypeResolver(typeof(MissingVirtualPropertyBase))).Generate([pagedDescriptor])).Message);
        }

        public sealed class Customer : BaseEntity
        {
            public Customer Parent { get; set; }
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

        public sealed class DeleteCustomerWithResponseCommand : IRequest<CustomerResponse>, TurtlePath.Models.Requests.IBaseRequest<CId>
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

        public sealed class GetCustomersQuery : GenericGetManyQuery<Customer, CustomerResponse, CId>
        {
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
            => new(CreateGenerator(),
                new AutomationHandlerServiceTypeResolver(),
                new Options.AutomationQueryOptionsRegistration(),
                new Options.AutomationCommandResponseOptionsRegistration());

        private static DynaBeeAutomationHandlerTypeGenerator CreateGenerator(
            IAutomationHandlerBaseTypeResolver baseTypeResolver = null)
            => new(
                new DynaBee.FluentApi.DependencyInjection.DynaBeeAssemblyBuilderFactory(),
                new AutomationHandlerGenerationOptions(),
                baseTypeResolver ?? new AutomationHandlerBaseTypeResolver(),
                new DefaultAutomationHandlerTypeNamePolicy());

        private static System.Linq.Expressions.Expression<Func<Customer, object>> Expression(System.Linq.Expressions.Expression<Func<Customer, object>> expression)
            => expression;

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

        private sealed class StubBaseTypeResolver(Type baseType) : IAutomationHandlerBaseTypeResolver
        {
            public Type Resolve(AutomationDescriptor descriptor)
                => baseType;
        }

        private sealed class MissingServiceProviderConstructorBase
        {
        }

        private abstract class MissingVirtualMethodBase
        {
            protected MissingVirtualMethodBase(IServiceProvider serviceProvider)
            {
            }
        }

        private abstract class MissingVirtualPropertyBase
        {
            protected MissingVirtualPropertyBase(IServiceProvider serviceProvider)
            {
            }
        }

        private sealed class DescriptorCommandResponseOptionsProxy
        {
            public DescriptorCommandResponseOptionsProxy(object value)
            {
                Value = Assert.IsAssignableFrom<ICommandResponseOptions<CreateCustomerCommand, Customer>>(value);
            }

            public ICommandResponseOptions<CreateCustomerCommand, Customer> Value { get; }
        }
    }
}
