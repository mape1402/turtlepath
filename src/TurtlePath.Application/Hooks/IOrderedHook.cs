namespace TurtlePath.Application.Hooks
{
    /// <summary>
    /// Defines an optional execution order for hooks resolved for the same handler stage.
    /// </summary>
    public interface IOrderedHook
    {
        /// <summary>
        /// Gets the execution order. Lower values run first.
        /// </summary>
        int Order { get; }
    }
}
