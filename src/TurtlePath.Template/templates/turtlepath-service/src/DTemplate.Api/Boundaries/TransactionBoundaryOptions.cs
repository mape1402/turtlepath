using System.Transactions;

namespace DTemplate.Api.Boundaries
{
    /// <summary>
    /// Configures the Spider transaction boundary.
    /// </summary>
    public sealed class TransactionBoundaryOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the transaction boundary is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether query requests should run inside a transaction.
        /// </summary>
        public bool IncludeQueries { get; set; }

        /// <summary>
        /// Gets or sets the transaction isolation level.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

        /// <summary>
        /// Gets or sets the transaction timeout in seconds. When null, the maximum transaction timeout is used.
        /// </summary>
        public int? TimeoutSeconds { get; set; }

        /// <summary>
        /// Gets request type names that should skip the transaction boundary.
        /// </summary>
        public HashSet<string> ExcludedRequestTypes { get; set; } = new(StringComparer.Ordinal);
    }
}
