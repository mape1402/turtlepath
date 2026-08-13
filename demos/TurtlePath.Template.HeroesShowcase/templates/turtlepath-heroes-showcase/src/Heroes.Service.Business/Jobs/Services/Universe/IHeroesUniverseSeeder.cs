namespace Heroes.Service.Business.Jobs.Services.Universe;

/// <summary>
/// Seeds the showcase domain without exposing EF Core details to the job that triggers it.
/// </summary>
public interface IHeroesUniverseSeeder
{
    /// <summary>
    /// Creates demo teams, heroes, villains, skills and incidents when the database is empty.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that resolves to <c>true</c> when data was created.</returns>
    Task<bool> SeedAsync(CancellationToken cancellationToken = default);
}
