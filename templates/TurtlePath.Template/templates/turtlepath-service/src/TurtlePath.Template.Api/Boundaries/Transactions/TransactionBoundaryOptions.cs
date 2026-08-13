using System.Transactions;
using System.Reflection;

namespace TurtlePath.Template.Api.Boundaries.Transactions
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

        /// <summary>
        /// Gets assemblies that should be scanned to cache transaction boundary decisions.
        /// </summary>
        public HashSet<Assembly> RequestAssemblies { get; } = new();

        /// <summary>
        /// Adds the assembly that contains the supplied request type to the discovery list.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <returns>The current options instance.</returns>
        public TransactionBoundaryOptions DiscoverRequestsFrom<TRequest>()
        {
            RequestAssemblies.Add(typeof(TRequest).Assembly);

            return this;
        }

        /// <summary>
        /// Adds an assembly to the request discovery list.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>The current options instance.</returns>
        public TransactionBoundaryOptions DiscoverRequestsFrom(Assembly assembly)
        {
            if (assembly != null)
                RequestAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Excludes a request type from the transaction boundary.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <returns>The current options instance.</returns>
        public TransactionBoundaryOptions Exclude<TRequest>()
        {
            ExcludedRequestTypes.Add(typeof(TRequest).FullName);

            return this;
        }
    }
}
