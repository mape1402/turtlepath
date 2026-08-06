namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleTransientException : Exception
{
    public SampleTransientException(string message)
        : base(message)
    {
    }
}
