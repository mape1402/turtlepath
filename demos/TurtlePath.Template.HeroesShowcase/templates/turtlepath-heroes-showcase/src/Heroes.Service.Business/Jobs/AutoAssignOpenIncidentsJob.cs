using Heroes.Service.Business.Incidents.Services.Backlog;
using TurtlePath.Jobs;

namespace Heroes.Service.Business.Jobs;

/// <summary>
/// Cron job that periodically assigns open incidents to the best available hero.
/// </summary>
public sealed class AutoAssignOpenIncidentsJob : TurtlePathJob
{
    private readonly IIncidentBacklogService _incidentBacklogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoAssignOpenIncidentsJob"/> class.
    /// </summary>
    public AutoAssignOpenIncidentsJob(IIncidentBacklogService incidentBacklogService)
    {
        _incidentBacklogService = incidentBacklogService;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        await _incidentBacklogService.AutoAssignReportedIncidentsAsync(10, cancellationToken);
    }
}
