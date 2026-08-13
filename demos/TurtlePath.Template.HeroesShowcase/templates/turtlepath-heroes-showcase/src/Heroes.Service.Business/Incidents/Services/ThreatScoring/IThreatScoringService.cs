using Heroes.Service.Domain.Enums;

namespace Heroes.Service.Business.Incidents.Services.ThreatScoring;

/// <summary>
/// Converts incident context into a normalized threat score.
/// </summary>
public interface IThreatScoringService
{
    /// <summary>
    /// Calculates the score used by custom handlers and jobs.
    /// </summary>
    int CalculateScore(ThreatLevel threatLevel, int? villainPowerLevel = null);
}
