namespace TurtlePath.Samples.Basic.Application.EventSourcing;

public sealed record CustomerEventSource(string CustomerId, string Name, string Email);
