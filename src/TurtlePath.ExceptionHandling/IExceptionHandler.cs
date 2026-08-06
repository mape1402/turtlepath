namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Resolves exceptions into transport-neutral descriptors.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Resolves the specified exception into a descriptor.
        /// </summary>
        ExceptionDescriptor Handle(Exception exception, ExceptionHandlingContext context = null);
    }
}
