using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Persistence.Repositories.Heroes;

/// <summary>
/// Reads the operations report from a storage-specific optimized query.
/// </summary>
public interface IHeroOperationsReadRepository
{
    /// <summary>
    /// Builds the active hero operations rows using the persistence mechanism selected by the infrastructure layer.
    /// </summary>
    /// <param name="teamId">Optional team filter.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The rows used by the operations report.</returns>
    Task<IReadOnlyList<HeroOperationsReadRow>> GetActiveHeroOperationsAsync(CId? teamId, CancellationToken cancellationToken = default);
}
