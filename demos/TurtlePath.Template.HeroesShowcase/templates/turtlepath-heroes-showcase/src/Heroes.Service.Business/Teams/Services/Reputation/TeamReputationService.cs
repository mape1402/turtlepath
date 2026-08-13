using Heroes.Service.Business.Services.Audit;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TeamEntity = Heroes.Service.Domain.Team;

namespace Heroes.Service.Business.Teams.Services.Reputation;

/// <summary>
/// EF-backed reputation calculator hidden behind a feature service.
/// </summary>
public sealed class TeamReputationService : ITeamReputationService
{
    private readonly IDbContext _dbContext;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeamReputationService"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context abstraction.</param>
    /// <param name="auditTrail">The demo audit trail.</param>
    public TeamReputationService(IDbContext dbContext, IAuditTrail auditTrail)
    {
        _dbContext = dbContext;
        _auditTrail = auditTrail;
    }

    /// <inheritdoc />
    public async Task<int> RecalculateAllAsync(CancellationToken cancellationToken = default)
    {
        var teams = await _dbContext.Set<TeamEntity>()
            .Include(team => team.Heroes)
            .Include(team => team.Villains)
            .ToListAsync(cancellationToken);

        foreach (var team in teams)
        {
            var heroScore = team.Heroes.Where(hero => hero.Active).Sum(hero => hero.PowerLevel);
            var villainPenalty = team.Villains.Where(villain => !villain.Captured).Sum(villain => villain.PowerLevel);
            team.Reputation = Math.Clamp(heroScore - villainPenalty, -100, 100);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _auditTrail.Add("Recalculated team reputation.");

        return teams.Count;
    }
}
