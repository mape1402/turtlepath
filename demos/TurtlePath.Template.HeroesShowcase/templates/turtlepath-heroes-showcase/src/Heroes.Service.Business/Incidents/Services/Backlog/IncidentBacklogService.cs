using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Incidents.Services.Assignment;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Incidents.Services.Backlog;

/// <summary>
/// EF-backed service for background incident assignment work.
/// </summary>
public sealed class IncidentBacklogService : IIncidentBacklogService
{
    private readonly IDbContext _dbContext;
    private readonly IIncidentAssignmentService _assignmentService;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentBacklogService"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context abstraction.</param>
    /// <param name="assignmentService">The assignment domain service.</param>
    /// <param name="auditTrail">The demo audit trail.</param>
    public IncidentBacklogService(IDbContext dbContext, IIncidentAssignmentService assignmentService, IAuditTrail auditTrail)
    {
        _dbContext = dbContext;
        _assignmentService = assignmentService;
        _auditTrail = auditTrail;
    }

    /// <inheritdoc />
    public async Task<int> AutoAssignReportedIncidentsAsync(int take, CancellationToken cancellationToken = default)
    {
        var incidents = await _dbContext.Set<IncidentEntity>()
            .Where(incident => incident.Status == IncidentStatus.Reported)
            .OrderByDescending(incident => incident.ThreatLevel)
            .Take(take)
            .ToListAsync(cancellationToken);

        foreach (var incident in incidents)
        {
            var hero = await _assignmentService.SelectBestHeroAsync(incident, cancellationToken);
            await _assignmentService.AssignAsync(incident, hero.Id, cancellationToken);
            _auditTrail.Add($"Auto-assigned incident '{incident.Title}' to '{hero.Alias}'.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return incidents.Count;
    }
}
