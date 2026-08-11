using Heroes.Service.Business.Services.Audit;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Jobs;
using TeamEntity = Heroes.Service.Domain.Team;

namespace Heroes.Service.Business.Jobs;

/// <summary>
/// Cron job that recalculates team reputation from member power and incident outcomes.
/// </summary>
public sealed class RecalculateTeamReputationJob : TurtlePathJob
{
    private readonly IDbContext _dbContext;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecalculateTeamReputationJob"/> class.
    /// </summary>
    public RecalculateTeamReputationJob(IDbContext _dbContext, IAuditTrail _auditTrail)
    {
        this._dbContext = _dbContext;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
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
    }
}
