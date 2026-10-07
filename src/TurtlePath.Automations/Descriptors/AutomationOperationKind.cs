namespace TurtlePath.Automations.Descriptors
{
    /// <summary>
    /// Represents the operation implemented by an automated TurtlePath handler.
    /// </summary>
    public enum AutomationOperationKind
    {
        /// <summary>
        /// Creates a new entity.
        /// </summary>
        Create,

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        Update,

        /// <summary>
        /// Deletes an existing entity.
        /// </summary>
        Delete,

        /// <summary>
        /// Applies a partial update to an existing entity.
        /// </summary>
        Patch,

        /// <summary>
        /// Reads one entity by identifier.
        /// </summary>
        GetById,

        /// <summary>
        /// Reads one entity by a custom filter.
        /// </summary>
        GetOne,

        /// <summary>
        /// Reads a collection of entities.
        /// </summary>
        GetMany,

        /// <summary>
        /// Reads a paged collection of entities.
        /// </summary>
        GetPaged
    }
}
