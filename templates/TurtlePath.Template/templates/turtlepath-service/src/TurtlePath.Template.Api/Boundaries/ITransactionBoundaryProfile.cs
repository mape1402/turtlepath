namespace TurtlePath.Template.Api.Boundaries
{
    /// <summary>
    /// Configures transaction boundary behavior without changing template defaults.
    /// </summary>
    public interface ITransactionBoundaryProfile
    {
        /// <summary>
        /// Applies transaction boundary options for a service, module, or feature.
        /// </summary>
        /// <param name="options">The transaction boundary options.</param>
        void Configure(TransactionBoundaryOptions options);
    }
}
