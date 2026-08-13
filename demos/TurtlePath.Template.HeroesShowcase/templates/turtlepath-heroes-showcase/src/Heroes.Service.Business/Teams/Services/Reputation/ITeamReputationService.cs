namespace Heroes.Service.Business.Teams.Services.Reputation;

/// <summary>
/// Encapsulates team reputation recalculation for recurring jobs and manual maintenance handlers.
/// </summary>
public interface ITeamReputationService
{
    /// <summary>
    /// Recalculates reputation for every team.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The number of teams updated.</returns>
    Task<int> RecalculateAllAsync(CancellationToken cancellationToken = default);
}
