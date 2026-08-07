using Krackend.EventSourcing.Contracts;

namespace TurtlePath.Samples.Basic.Application.Events;

[EventSchema("customer-email-patched")]
public sealed class CustomerEmailPatched
{
    public string CustomerId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
