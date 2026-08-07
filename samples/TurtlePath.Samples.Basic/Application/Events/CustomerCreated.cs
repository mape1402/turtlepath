using Krackend.EventSourcing.Contracts;

namespace TurtlePath.Samples.Basic.Application.Events;

[EventSchema("customer-created")]
public sealed class CustomerCreated
{
    public string CustomerId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
