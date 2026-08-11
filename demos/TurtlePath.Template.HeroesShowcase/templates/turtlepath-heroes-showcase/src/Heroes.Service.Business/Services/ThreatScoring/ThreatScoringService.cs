using Heroes.Service.Domain.Enums;

namespace Heroes.Service.Business.Services.ThreatScoring;

/// <summary>
/// Demo scoring service that shows where reusable business logic should live.
/// </summary>
public sealed class ThreatScoringService : IThreatScoringService
{
    /// <inheritdoc />
    public int CalculateScore(ThreatLevel threatLevel, int? villainPowerLevel = null)
    {
        var baseScore = threatLevel switch
        {
            ThreatLevel.Low => 15,
            ThreatLevel.Medium => 35,
            ThreatLevel.High => 65,
            ThreatLevel.Critical => 90,
            _ => 10
        };

        return Math.Min(100, baseScore + (villainPowerLevel ?? 0) / 10);
    }
}
