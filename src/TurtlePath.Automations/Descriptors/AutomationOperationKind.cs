namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Represents the operation implemented by an automated TurtlePath handler.
    /// </summary>
    internal enum AutomationOperationKind
    {
        Create,
        Update,
        Delete,
        Patch,
        GetById,
        GetOne,
        GetMany,
        GetPaged
    }
}
