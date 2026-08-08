namespace TurtlePath.DataScorpio.Tests;

using global::DataScorpio.Execution;
using global::DataScorpio.Parsing.Sieve;
using global::DataScorpio.Profiles;
using global::DataScorpio.Validation;
using TurtlePath.Domain.Contracts;
using TurtlePath.Persistence;

public sealed class DataScorpioStorageCriteriaApplierTests
{
    [Fact]
    public void Apply_filters_and_sorts_using_configured_profiles()
    {
        var applier = CreateApplier();
        var criteria = new GetManyCriteria<Customer>
        {
            Filters = "Status==Active",
            Sorts = "-CreatedAt"
        };

        var result = applier.Apply(Customers().AsQueryable(), criteria).ToArray();

        Assert.Equal(["Ada", "Grace"], result.Select(customer => customer.Name));
    }

    [Fact]
    public void Apply_returns_source_when_no_string_criteria_is_requested()
    {
        var applier = CreateApplier();
        var source = Customers().AsQueryable();

        var result = applier.Apply(source, new GetManyCriteria<Customer>());

        Assert.Same(source, result);
    }

    [Fact]
    public void Apply_throws_validation_exception_for_unknown_fields()
    {
        var applier = CreateApplier();
        var criteria = new GetManyCriteria<Customer>
        {
            Filters = "PasswordHash==secret"
        };

        var exception = Assert.Throws<DataScorpioTurtlePathQueryException>(() =>
            applier.Apply(Customers().AsQueryable(), criteria).ToArray());

        Assert.Contains(exception.Validation.Errors, error => error.Code == QueryValidationCodes.UnknownField);
    }

    private static DataScorpioStorageCriteriaApplier CreateApplier()
    {
        var registry = new QueryProfileRegistryBuilder()
            .AddProfile(new CustomerQueryProfile())
            .Build();

        return new DataScorpioStorageCriteriaApplier(
            new SieveQueryParser(),
            new QueryDescriptorValidator(),
            new QueryableQueryApplier(),
            registry);
    }

    private static IReadOnlyList<Customer> Customers()
        =>
        [
            new Customer { Id = 1, Name = "Ada", Status = "Active", CreatedAt = new DateTime(2026, 1, 3) },
            new Customer { Id = 2, Name = "Grace", Status = "Active", CreatedAt = new DateTime(2026, 1, 1) },
            new Customer { Id = 3, Name = "Alan", Status = "Inactive", CreatedAt = new DateTime(2026, 1, 2) }
        ];

    private sealed class CustomerQueryProfile : QueryProfile<Customer>
    {
        public override void Configure(IQueryProfileBuilder<Customer> builder)
        {
            builder
                .AllowFilter(customer => customer.Status)
                .AllowSort(customer => customer.CreatedAt);
        }
    }

    private sealed class Customer : IEntity
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public string Status { get; init; }

        public DateTime CreatedAt { get; init; }
    }
}
