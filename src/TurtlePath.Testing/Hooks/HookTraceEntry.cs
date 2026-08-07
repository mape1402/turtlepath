namespace TurtlePath.Testing.Hooks
{
    /// <summary>
    /// Describes one hook stage execution captured by the test hook trace.
    /// </summary>
    public sealed record HookTraceEntry(
        string Stage,
        Type RequestType,
        Type EntityType,
        Type ResponseType,
        object Request,
        object Entity,
        object Response);
}
