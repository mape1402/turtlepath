namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleExceptionReportLog
{
    private readonly List<string> entries = new();

    public IReadOnlyCollection<string> Entries => entries;

    public void Add(string entry)
    {
        entries.Add(entry);
    }
}
