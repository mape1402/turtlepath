namespace Heroes.Service.Business.Services.Audit;

/// <summary>
/// Records demo audit entries produced by hooks, handlers and jobs.
/// </summary>
public interface IAuditTrail
{
    /// <summary>
    /// Adds a new audit entry.
    /// </summary>
    void Add(string message);

    /// <summary>
    /// Gets all audit entries recorded during the current process lifetime.
    /// </summary>
    IReadOnlyCollection<string> Entries { get; }
}
