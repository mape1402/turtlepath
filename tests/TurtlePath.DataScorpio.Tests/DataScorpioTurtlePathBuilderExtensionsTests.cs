namespace TurtlePath.DataScorpio.Tests;

using Microsoft.Extensions.DependencyInjection;
using TurtlePath;
using TurtlePath.Domain.Contracts;
using TurtlePath.Persistence;

public sealed class DataScorpioTurtlePathBuilderExtensionsTests
{
    [Fact]
    public void UseDataScorpio_registers_storage_criteria_applier()
    {
        var services = new ServiceCollection();
        var builder = new TestTurtlePathBuilder(services);

        builder.UseDataScorpio(profiles =>
            profiles.AddProfile(new CustomerQueryProfile()));

        using var provider = services.BuildServiceProvider();

        Assert.Contains(
            provider.GetServices<IStorageCriteriaApplier>(),
            applier => applier is DataScorpioStorageCriteriaApplier);
    }

    private sealed class TestTurtlePathBuilder : ITurtlePathBuilder
    {
        public TestTurtlePathBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    private sealed class CustomerQueryProfile : global::DataScorpio.Profiles.QueryProfile<Customer>
    {
        public override void Configure(global::DataScorpio.Profiles.IQueryProfileBuilder<Customer> builder)
        {
            builder.AllowFilter(customer => customer.Name);
        }
    }

    private sealed class Customer : IEntity
    {
        public int Id { get; init; }

        public string Name { get; init; }
    }
}
