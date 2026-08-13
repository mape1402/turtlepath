using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Incidents.Services.Assignment;

/// <summary>
/// Owns incident assignment rules that are shared by handlers and jobs.
/// </summary>
public interface IIncidentAssignmentService
{
    /// <summary>
    /// Assigns an incident to the requested hero after applying business rules.
    /// </summary>
    Task AssignAsync(IncidentEntity incident, CId heroId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects a hero for an open incident using the demo scoring strategy.
    /// </summary>
    Task<Hero> SelectBestHeroAsync(IncidentEntity incident, CancellationToken cancellationToken = default);
}
