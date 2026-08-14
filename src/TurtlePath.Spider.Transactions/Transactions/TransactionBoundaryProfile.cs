namespace TurtlePath.Spider.Transactions;

/// <summary>
/// Base class for transaction boundary profiles.
/// </summary>
public abstract class TransactionBoundaryProfile : SpiderBoundaryProfile<TransactionBoundaryOptions>, ITransactionBoundaryProfile
{
    /// <inheritdoc />
    public abstract override void Configure(TransactionBoundaryOptions options);
}
