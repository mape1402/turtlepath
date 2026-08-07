namespace TurtlePath.Samples.Basic.Application.EventSourcing;

public sealed record CustomerAuditEventSource(string CustomerId, string Email);
