using Krackend.EventSourcing.Stores;
using TurtlePath.EventSourcing;
using TurtlePath.Samples.Basic.Application.Events;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.EventSourcing;

public sealed class CommerceEventSourcingProfile : IEventSourcingProfile
{
    public void Configure(IEventSourcingProfileBuilder builder)
    {
        builder.For<CreateCustomerRequest, Customer>()
            .UseStream("customers", context => context.Entity.Id.ToString())
            .ToEvent<CustomerEventSource, CustomerCreated>(
                ToCustomerEventSource,
                options => options.UseExpectedVersion(ExpectedVersion.NoStream))
            .ToEvent<CustomerAuditEventSource, CustomerAuditEventRegistered>(
                context => new CustomerAuditEventSource(context.Entity.Id.ToString(), context.Entity.Email),
                options => options.UseExpectedVersion(ExpectedVersion.NoStream));

        builder.For<UpdateCustomerRequest, Customer>()
            .UseStream("customers", context => context.Entity.Id.ToString())
            .ToEvent<CustomerEventSource, CustomerUpdated>(ToCustomerEventSource);

        builder.For<PatchCustomerEmailRequest, Customer>()
            .UseStream("customers", context => context.Entity.Id.ToString())
            .ToEvent<CustomerAuditEventSource, CustomerEmailPatched>(
                context => new CustomerAuditEventSource(context.Entity.Id.ToString(), context.Entity.Email));
    }

    private static CustomerEventSource ToCustomerEventSource<TRequest>(
        TurtlePath.Hooks.CommandHookContext<TRequest, Customer> context)
        where TRequest : class
    {
        return new CustomerEventSource(
            context.Entity.Id.ToString(),
            context.Entity.Name,
            context.Entity.Email);
    }
}
