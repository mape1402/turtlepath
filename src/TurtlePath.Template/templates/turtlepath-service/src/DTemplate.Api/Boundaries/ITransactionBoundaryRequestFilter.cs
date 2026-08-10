namespace DTemplate.Api.Boundaries
{
    /// <summary>
    /// Determines whether a request type should run inside the transaction boundary.
    /// </summary>
    public interface ITransactionBoundaryRequestFilter
    {
        /// <summary>
        /// Discovers and caches request boundary decisions from the supplied assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        void Discover(params System.Reflection.Assembly[] assemblies);

        /// <summary>
        /// Determines whether the specified request type should open a transaction.
        /// </summary>
        /// <param name="requestType">The request type.</param>
        /// <returns><c>true</c> when the request should run inside a transaction.</returns>
        bool ShouldOpenTransaction(Type requestType);
    }
}
