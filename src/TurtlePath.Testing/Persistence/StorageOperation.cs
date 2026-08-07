namespace TurtlePath.Testing.Persistence
{
    /// <summary>
    /// Describes an operation executed against the in-memory TurtlePath test storage.
    /// </summary>
    public sealed record StorageOperation(string Action, Type EntityType, object Entity);
}
