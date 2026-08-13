using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Persistence.Repositories.Heroes;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Heroes.Services.OperationsReport;

/// <summary>
/// Composes the hero operations report from the persistence read repository.
/// </summary>
public sealed class HeroOperationsReportService : IHeroOperationsReportService
{
    private readonly IHeroOperationsReadRepository _readRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroOperationsReportService"/> class.
    /// </summary>
    /// <param name="readRepository">The persistence read repository.</param>
    public HeroOperationsReportService(IHeroOperationsReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    /// <inheritdoc />
    public async Task<HeroOperationsReportResponse> GetOperationsReportAsync(CId? teamId, CancellationToken cancellationToken = default)
    {
        var rows = await _readRepository.GetActiveHeroOperationsAsync(teamId, cancellationToken);

        return new HeroOperationsReportResponse
        {
            ActiveHeroes = rows.Count,
            OpenAssignments = rows.Sum(row => row.AssignedOpenIncidents),
            Heroes = rows
                .Select(row => new HeroOperationsRowResponse
                {
                    Alias = row.Alias,
                    City = row.City,
                    PowerLevel = row.PowerLevel,
                    TeamName = row.TeamName,
                    AssignedOpenIncidents = row.AssignedOpenIncidents
                })
                .ToList()
        };
    }
}
