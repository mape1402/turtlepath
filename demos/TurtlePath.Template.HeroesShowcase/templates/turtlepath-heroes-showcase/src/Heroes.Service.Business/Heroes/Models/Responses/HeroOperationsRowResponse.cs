namespace Heroes.Service.Business.Heroes.Models.Responses;

/// <summary>
/// One row in the hero operations report.
/// </summary>
public sealed class HeroOperationsRowResponse
{
    /// <summary>
    /// Gets or sets the hero alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city where the hero operates.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the team name.
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hero power level.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets the number of assigned unresolved incidents.
    /// </summary>
    public int AssignedOpenIncidents { get; set; }
}
