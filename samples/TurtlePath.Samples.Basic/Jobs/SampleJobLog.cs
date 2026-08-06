namespace TurtlePath.Samples.Basic.Jobs;

public sealed class SampleJobLog
{
    private readonly List<string> entries = new();
    private readonly object sync = new();

    public IReadOnlyCollection<string> Entries
    {
        get
        {
            lock (sync)
                return entries.ToArray();
        }
    }

    public void Add(string entry)
    {
        lock (sync)
            entries.Add(entry);
    }
}
