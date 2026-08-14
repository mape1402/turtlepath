using System.Reflection;
using System.Transactions;

namespace TurtlePath.Spider.Transactions;

/// <summary>
/// Configures the Spider transaction boundary.
/// </summary>
public sealed class TransactionBoundaryOptions
{
    /// <summary>Gets or sets a value indicating whether the transaction boundary is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether query requests should run inside a transaction.</summary>
    public bool IncludeQueries { get; set; }

    /// <summary>Gets or sets the transaction isolation level.</summary>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <summary>Gets or sets the transaction timeout in seconds.</summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>Gets request type names that should skip the transaction boundary.</summary>
    public HashSet<string> ExcludedRequestTypes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>Gets assemblies that should be scanned for request types.</summary>
    public HashSet<Assembly> RequestAssemblies { get; } = new();

    /// <summary>Adds the assembly that contains the supplied request type to discovery.</summary>
    public TransactionBoundaryOptions DiscoverRequestsFrom<TRequest>()
    {
        RequestAssemblies.Add(typeof(TRequest).Assembly);
        return this;
    }

    /// <summary>Adds an assembly to request discovery.</summary>
    public TransactionBoundaryOptions DiscoverRequestsFrom(Assembly assembly)
    {
        if (assembly != null)
            RequestAssemblies.Add(assembly);

        return this;
    }

    /// <summary>Excludes a request type from the transaction boundary.</summary>
    public TransactionBoundaryOptions Exclude<TRequest>()
    {
        ExcludedRequestTypes.Add(typeof(TRequest).FullName);
        return this;
    }
}
