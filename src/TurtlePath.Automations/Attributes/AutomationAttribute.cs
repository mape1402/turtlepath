namespace TurtlePath.Automations.Attributes
{
    using TurtlePath.Automations.Descriptors;

    /// <summary>
    /// Base attribute for simple TurtlePath automation declarations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public abstract class AutomationAttribute : Attribute
    {
        private protected AutomationAttribute(Type entityType, Type responseType = null)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            ResponseType = responseType;
        }

        /// <summary>
        /// Gets the entity type automated by the request.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the response type, when the operation returns one.
        /// </summary>
        public Type ResponseType { get; }

        internal abstract AutomationOperationKind OperationKind { get; }
    }
}
