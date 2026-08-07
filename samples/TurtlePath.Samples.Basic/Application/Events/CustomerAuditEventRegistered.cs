using Krackend.EventSourcing.Contracts;

namespace TurtlePath.Samples.Basic.Application.Events;

[EventSchema("customer-audit-event-registered")]
public sealed class CustomerAuditEventRegistered
{
    public string CustomerId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
