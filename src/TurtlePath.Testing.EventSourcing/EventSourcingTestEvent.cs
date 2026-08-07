namespace TurtlePath.Testing.EventSourcing
{
    /// <summary>
    /// Assertion-friendly view of an event stored in the event store.
    /// </summary>
    public sealed record EventSourcingTestEvent(
        string EventType,
        long StreamVersion,
        string Payload,
        IReadOnlyDictionary<string, object> Metadata);
}
