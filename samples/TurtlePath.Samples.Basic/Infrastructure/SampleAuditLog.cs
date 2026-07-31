namespace TurtlePath.Samples.Basic.Infrastructure;

public sealed class SampleAuditLog
{
    private readonly List<string> entries = [];

    public IReadOnlyList<string> Entries => entries;

    public void Add(string entry)
    {
        entries.Add(entry);
    }
}
