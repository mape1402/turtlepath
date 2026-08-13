namespace Heroes.Service.Business.Incidents.Services.Backlog;

/// <summary>
/// Encapsulates recurring incident backlog work for cron jobs.
/// </summary>
public interface IIncidentBacklogService
{
    /// <summary>
    /// Assigns the highest priority reported incidents to eligible heroes.
    /// </summary>
    /// <param name="take">The maximum number of incidents to process.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The number of incidents assigned.</returns>
    Task<int> AutoAssignReportedIncidentsAsync(int take, CancellationToken cancellationToken = default);
}
