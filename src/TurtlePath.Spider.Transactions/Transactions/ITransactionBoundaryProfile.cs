namespace TurtlePath.Spider.Transactions;

/// <summary>
/// Configures transaction boundary behavior without changing application defaults.
/// </summary>
public interface ITransactionBoundaryProfile : ISpiderBoundaryProfile<TransactionBoundaryOptions>
{
}
