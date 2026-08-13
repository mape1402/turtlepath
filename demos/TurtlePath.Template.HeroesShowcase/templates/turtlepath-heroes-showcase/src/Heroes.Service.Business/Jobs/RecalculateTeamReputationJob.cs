using Heroes.Service.Business.Teams.Services.Reputation;
using TurtlePath.Jobs;

namespace Heroes.Service.Business.Jobs;

/// <summary>
/// Cron job that recalculates team reputation from member power and incident outcomes.
/// </summary>
public sealed class RecalculateTeamReputationJob : TurtlePathJob
{
    private readonly ITeamReputationService _teamReputationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecalculateTeamReputationJob"/> class.
    /// </summary>
    public RecalculateTeamReputationJob(ITeamReputationService teamReputationService)
    {
        _teamReputationService = teamReputationService;
    }

    /// <inheritdoc />
    public override async Task ExecuteAsync(TurtlePathJobContext context, CancellationToken cancellationToken)
    {
        await _teamReputationService.RecalculateAllAsync(cancellationToken);
    }
}
