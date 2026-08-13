using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TurtlePath.Domain.Identifier;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Exceptions;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Incidents.Services.Assignment;

/// <summary>
/// Default incident assignment service used by custom handlers and cron jobs.
/// </summary>
public sealed class IncidentAssignmentService : IIncidentAssignmentService
{
    private readonly IDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentAssignmentService"/> class.
    /// </summary>
    public IncidentAssignmentService(IDbContext _dbContext)
    {
        this._dbContext = _dbContext;
    }

    /// <inheritdoc />
    public async Task AssignAsync(IncidentEntity incident, CId heroId, CancellationToken cancellationToken = default)
    {
        var hero = await _dbContext.Set<Hero>().FirstOrDefaultAsync(item => item.Id == heroId && item.Active, cancellationToken)
            ?? throw new NotFoundException(nameof(Hero), heroId.ToString());

        if (hero.PowerLevel < RequiredPower(incident.ThreatLevel))
            throw new InvalidOperationException($"Hero '{hero.Alias}' is not strong enough for a {incident.ThreatLevel} incident.");

        incident.AssignedHeroId = hero.Id;
        incident.Status = IncidentStatus.Assigned;
    }

    /// <inheritdoc />
    public async Task<Hero> SelectBestHeroAsync(IncidentEntity incident, CancellationToken cancellationToken = default)
    {
        var requiredPower = RequiredPower(incident.ThreatLevel);
        var hero = await _dbContext.Set<Hero>()
            .Where(item => item.Active && item.PowerLevel >= requiredPower)
            .OrderByDescending(item => item.PowerLevel)
            .FirstOrDefaultAsync(cancellationToken);

        return hero ?? throw new InvalidOperationException($"No active hero can handle a {incident.ThreatLevel} incident.");
    }

    private static int RequiredPower(ThreatLevel threatLevel)
        => threatLevel switch
        {
            ThreatLevel.Low => 1,
            ThreatLevel.Medium => 35,
            ThreatLevel.High => 60,
            ThreatLevel.Critical => 85,
            _ => 1
        };
}
