namespace Heroes.Service.Domain.Enums;

/// <summary>
/// Defines the incident lifecycle used by automations, custom handlers and cron jobs.
/// </summary>
public enum IncidentStatus
{
    /// <summary>
    /// Incident was reported and still needs assignment.
    /// </summary>
    Reported = 1,

    /// <summary>
    /// Incident has an assigned hero.
    /// </summary>
    Assigned = 2,

    /// <summary>
    /// Incident has been resolved.
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Incident has been archived.
    /// </summary>
    Archived = 4
}
