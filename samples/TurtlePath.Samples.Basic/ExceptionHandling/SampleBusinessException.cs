namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleBusinessException : Exception
{
    public SampleBusinessException(string message)
        : base(message)
    {
    }
}
