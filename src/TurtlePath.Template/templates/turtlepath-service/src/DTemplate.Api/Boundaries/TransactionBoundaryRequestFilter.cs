using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Reflection;

namespace DTemplate.Api.Boundaries
{
    /// <summary>
    /// Caches transaction boundary decisions by request type.
    /// </summary>
    public sealed class TransactionBoundaryRequestFilter : ITransactionBoundaryRequestFilter
    {
        private readonly ConcurrentDictionary<Type, bool> decisions = new();
        private readonly TransactionBoundaryOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionBoundaryRequestFilter"/> class.
        /// </summary>
        /// <param name="options">The transaction boundary options.</param>
        public TransactionBoundaryRequestFilter(IOptions<TransactionBoundaryOptions> options)
        {
            this.options = options?.Value ?? new TransactionBoundaryOptions();
        }

        /// <inheritdoc />
        public void Discover(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                return;

            foreach (var type in assemblies
                .Where(assembly => assembly != null)
                .SelectMany(GetLoadableTypes)
                .Where(type => type is { IsAbstract: false, IsClass: true }))
            {
                decisions.TryAdd(type, CreateDecision(type));
            }
        }

        /// <inheritdoc />
        public bool ShouldOpenTransaction(Type requestType)
        {
            if (!options.Enabled || requestType == null)
                return false;

            return decisions.GetOrAdd(requestType, CreateDecision);
        }

        private bool CreateDecision(Type requestType)
        {
            if (!options.IncludeQueries && IsQuery(requestType))
                return false;

            if (Attribute.IsDefined(requestType, typeof(SkipTransactionBoundaryAttribute), inherit: false))
                return false;

            return !IsExcluded(requestType);
        }

        private bool IsExcluded(Type requestType)
        {
            if (options.ExcludedRequestTypes == null || options.ExcludedRequestTypes.Count == 0)
                return false;

            return options.ExcludedRequestTypes.Contains(requestType.FullName) ||
                   options.ExcludedRequestTypes.Contains(requestType.Name);
        }

        private static bool IsQuery(Type requestType)
            => requestType.Name.EndsWith("Query", StringComparison.Ordinal) ||
               (requestType.Namespace?.Split('.').Any(part => string.Equals(part, "Queries", StringComparison.Ordinal)) ?? false);

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
