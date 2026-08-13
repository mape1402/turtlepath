namespace Heroes.Service.Persistence.Repositories.Heroes;

/// <summary>
/// Persistence read model returned by the optimized hero operations repository.
/// </summary>
public sealed class HeroOperationsReadRow
{
    /// <summary>
    /// Gets or sets the hero alias.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city protected by the hero.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hero power level.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets the team name.
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of assigned unresolved incidents.
    /// </summary>
    public int AssignedOpenIncidents { get; set; }
}
