namespace TurtlePath.Template.Api.Boundaries
{
    /// <summary>
    /// Base class for transaction boundary profiles.
    /// </summary>
    public abstract class TransactionBoundaryProfile : ITransactionBoundaryProfile
    {
        /// <inheritdoc />
        public abstract void Configure(TransactionBoundaryOptions options);
    }
}
