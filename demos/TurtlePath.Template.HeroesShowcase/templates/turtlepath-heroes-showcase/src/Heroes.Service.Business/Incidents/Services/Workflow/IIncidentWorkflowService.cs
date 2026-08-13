using Heroes.Service.Business.Incidents.Models.Requests;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Incidents.Services.Workflow;

/// <summary>
/// Encapsulates incident state transitions that need persistence and business checks.
/// </summary>
public interface IIncidentWorkflowService
{
    /// <summary>
    /// Assigns an incident to a hero.
    /// </summary>
    /// <param name="request">The assignment request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The updated incident entity.</returns>
    Task<IncidentEntity> AssignAsync(AssignIncidentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an assigned incident.
    /// </summary>
    /// <param name="request">The resolve request.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The updated incident entity.</returns>
    Task<IncidentEntity> ResolveAsync(ResolveIncidentRequest request, CancellationToken cancellationToken = default);
}
