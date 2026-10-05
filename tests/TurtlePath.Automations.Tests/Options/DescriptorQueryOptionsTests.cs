namespace TurtlePath.Automations.Tests.QueryOptions
{
    using System.Linq.Expressions;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Options;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    public sealed class DescriptorQueryOptionsTests
    {
        [Fact]
        public void Get_one_options_build_filter_from_descriptor_key_selector()
        {
            var registry = new AutomationDescriptorRegistry([
                new AutomationDescriptor(
                    AutomationOperationKind.GetOne,
                    typeof(GetCustomerByCodeQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse),
                    keySelector: (Expression<Func<GetCustomerByCodeQuery, int>>)(query => query.Code))
            ]);
            var options = new DescriptorGetOneQueryOptions<GetCustomerByCodeQuery, Customer, int, int>(registry);

            var filter = options.GetFilterExpression(new GetCustomerByCodeQuery { Code = 5 }).Compile();

            Assert.True(filter(new Customer { Id = 5 }));
            Assert.False(filter(new Customer { Id = 6 }));
        }

        [Fact]
        public void Get_one_options_build_filter_from_value_property()
        {
            var options = new DescriptorGetOneQueryOptions<GetCustomerByValueQuery, Customer, int, int>(
                new AutomationDescriptorRegistry());

            var filter = options.GetFilterExpression(new GetCustomerByValueQuery { Value = 9 }).Compile();

            Assert.True(filter(new Customer { Id = 9 }));
            Assert.False(filter(new Customer { Id = 10 }));
        }

        [Fact]
        public void Get_one_options_require_value_property_or_compatible_key()
        {
            var noValue = new DescriptorGetOneQueryOptions<GetCustomerWithoutValueQuery, Customer, int, int>(
                new AutomationDescriptorRegistry());
            var incompatible = new DescriptorGetOneQueryOptions<GetCustomerByStringValueQuery, Customer, int, string>(
                new AutomationDescriptorRegistry());

            Assert.Throws<NotSupportedException>(() => noValue.GetFilterExpression(new GetCustomerWithoutValueQuery()));
            Assert.Throws<NotSupportedException>(() => incompatible.GetFilterExpression(new GetCustomerByStringValueQuery { Value = "9" }));
        }

        [Fact]
        public void Get_many_options_resolve_default_sort_from_descriptor()
        {
            var registry = new AutomationDescriptorRegistry([
                new AutomationDescriptor(
                    AutomationOperationKind.GetMany,
                    typeof(GetCustomersQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(IEnumerable<CustomerResponse>),
                    defaultSortProperty: "-Name")
            ]);

            var options = new DescriptorGetManyQueryOptions<GetCustomersQuery, Customer, CustomerResponse>(registry);
            var missing = new DescriptorGetManyQueryOptions<GetUnknownCustomersQuery, Customer, CustomerResponse>(registry);

            Assert.Equal("-Name", options.DefaultSorts);
            Assert.Null(missing.DefaultSorts);
        }

        [Fact]
        public void Get_paged_options_resolve_default_sort_from_descriptor()
        {
            var registry = new AutomationDescriptorRegistry([
                new AutomationDescriptor(
                    AutomationOperationKind.GetPaged,
                    typeof(SearchCustomersQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(PagedResponse<CustomerResponse>),
                    defaultSortProperty: "Name")
            ]);

            var options = new DescriptorGetPagedInfoQueryOptions<SearchCustomersQuery, Customer, CustomerResponse>(registry);
            var missing = new DescriptorGetPagedInfoQueryOptions<SearchUnknownCustomersQuery, Customer, CustomerResponse>(registry);

            Assert.Equal("Name", options.DefaultSorts);
            Assert.Null(missing.DefaultSorts);
            Assert.Throws<ArgumentNullException>(() =>
                new DescriptorGetPagedInfoQueryOptions<SearchCustomersQuery, Customer, CustomerResponse>(null));
        }

        [Fact]
        public void Query_options_validate_registry_argument()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DescriptorGetOneQueryOptions<GetCustomerByValueQuery, Customer, int, int>(null));
            Assert.Throws<ArgumentNullException>(() =>
                new DescriptorGetManyQueryOptions<GetCustomersQuery, Customer, CustomerResponse>(null));
        }

        [Fact]
        public void Query_options_registration_registers_supported_query_option_services()
        {
            var services = new ServiceCollection();
            var descriptors = new[]
            {
                new AutomationDescriptor(
                    AutomationOperationKind.GetOne,
                    typeof(GetCustomerByGenericValueQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse)),
                new AutomationDescriptor(
                    AutomationOperationKind.GetOne,
                    typeof(GetCustomerByLongCodeQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse),
                    keySelector: (Expression<Func<GetCustomerByLongCodeQuery, long>>)(query => query.Code)),
                new AutomationDescriptor(
                    AutomationOperationKind.GetMany,
                    typeof(GetCustomersQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(IEnumerable<CustomerResponse>)),
                new AutomationDescriptor(
                    AutomationOperationKind.GetMany,
                    typeof(GetCustomersDirectResponseQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse)),
                new AutomationDescriptor(
                    AutomationOperationKind.GetPaged,
                    typeof(SearchCustomersQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(PagedResponse<CustomerResponse>)),
                new AutomationDescriptor(
                    AutomationOperationKind.GetPaged,
                    typeof(SearchCustomersDirectResponseQuery),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse)),
                new AutomationDescriptor(
                    AutomationOperationKind.Create,
                    typeof(CreateCustomerCommand),
                    typeof(Customer),
                    typeof(int),
                    AutomationReturnMode.Response,
                    typeof(CustomerResponse))
            };
            var registration = new AutomationQueryOptionsRegistration();

            registration.Register(services, descriptors);

            Assert.Contains(services, service => service.ServiceType == typeof(IGetOneQueryOptions<GetCustomerByGenericValueQuery, Customer>));
            Assert.Contains(services, service => service.ServiceType == typeof(IGetOneQueryOptions<GetCustomerByLongCodeQuery, Customer>));
            Assert.Contains(services, service => service.ServiceType == typeof(IGetManyQueryOptions<GetCustomersQuery, Customer>));
            Assert.Contains(services, service => service.ServiceType == typeof(IGetManyQueryOptions<GetCustomersDirectResponseQuery, Customer>));
            Assert.Contains(services, service => service.ServiceType == typeof(IGetPagedInfoQueryOptions<SearchCustomersQuery, Customer>));
            Assert.Contains(services, service => service.ServiceType == typeof(IGetPagedInfoQueryOptions<SearchCustomersDirectResponseQuery, Customer>));
            Assert.DoesNotContain(services, service => service.ServiceType == typeof(IGetOneQueryOptions<CreateCustomerCommand, Customer>));
            Assert.Throws<ArgumentNullException>(() => registration.Register(null, descriptors));
            Assert.Throws<ArgumentNullException>(() => registration.Register(services, null));
        }

        [Fact]
        public void Command_response_options_registration_validates_arguments()
        {
            var registration = new AutomationCommandResponseOptionsRegistration();

            Assert.Throws<ArgumentNullException>(() => registration.Register(null, []));
            Assert.Throws<ArgumentNullException>(() => registration.Register(new ServiceCollection(), null));
        }

        private sealed class Customer : IEntity<int>
        {
            public int Id { get; set; }
        }

        private sealed class CustomerResponse : IBaseResponse<int>
        {
            public int Id { get; set; }
        }

        private sealed class GetCustomerByCodeQuery
        {
            public int Code { get; set; }
        }

        private sealed class GetCustomerByLongCodeQuery
        {
            public long Code { get; set; }
        }

        private sealed class GetCustomerByValueQuery
        {
            public int Value { get; set; }
        }

        private sealed class GetCustomerByGenericValueQuery : GenericGetOneQuery<short, Customer, CustomerResponse, int>
        {
        }

        private sealed class GetCustomerByStringValueQuery
        {
            public string Value { get; set; }
        }

        private sealed class GetCustomerWithoutValueQuery
        {
        }

        private sealed class GetCustomersQuery
        {
        }

        private sealed class GetCustomersDirectResponseQuery
        {
        }

        private sealed class GetUnknownCustomersQuery
        {
        }

        private sealed class SearchCustomersQuery
        {
        }

        private sealed class SearchCustomersDirectResponseQuery
        {
        }

        private sealed class SearchUnknownCustomersQuery
        {
        }

        private sealed class CreateCustomerCommand
        {
        }
    }
}
