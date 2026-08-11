namespace Heroes.Service.Business.Services.Audit;

/// <summary>
/// Simple in-memory audit trail used by the demo to make hook and job behavior visible.
/// </summary>
public sealed class InMemoryAuditTrail : IAuditTrail
{
    private readonly List<string> _entries = [];

    /// <inheritdoc />
    public IReadOnlyCollection<string> Entries => _entries;

    /// <inheritdoc />
    public void Add(string message)
    {
        _entries.Add($"{DateTimeOffset.UtcNow:O} {message}");
    }
}
