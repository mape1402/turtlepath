using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Services.Incident;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Jobs;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Jobs;

/// <summary>
/// Cron job that periodically assigns open incidents to the best available hero.
/// </summary>
public sealed class AutoAssignOpenIncidentsJob : TurtlePathJob
{
    private readonly IDbContext _dbContext;
    private readonly IIncidentAssignmentService _assignmentService;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoAssignOpenIncidentsJob"/> class.
    /// </summary>
    public AutoAssignOpenIncidentsJob(IDbContext _dbContext, IIncidentAssignmentService _assignmentService, IAuditTrail _auditTrail)
    {
        this._dbContext = _dbContext;
        this._assignmentService = _assignmentService;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        var incidents = await _dbContext.Set<IncidentEntity>()
            .Where(incident => incident.Status == IncidentStatus.Reported)
            .OrderByDescending(incident => incident.ThreatLevel)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var incident in incidents)
        {
            var hero = await _assignmentService.SelectBestHeroAsync(incident, cancellationToken);
            await _assignmentService.AssignAsync(incident, hero.Id, cancellationToken);
            _auditTrail.Add($"Auto-assigned incident '{incident.Title}' to '{hero.Alias}'.");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
