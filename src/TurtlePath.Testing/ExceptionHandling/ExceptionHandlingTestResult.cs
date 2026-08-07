namespace TurtlePath.Testing.ExceptionHandling
{
    using TurtlePath.ExceptionHandling;

    /// <summary>
    /// Contains the descriptor resolved for an exception in a test scenario.
    /// </summary>
    public sealed record ExceptionHandlingTestResult(Exception Exception, ExceptionDescriptor Descriptor);
}
