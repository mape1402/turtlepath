using TurtlePath.Domain.Contracts;

namespace Heroes.Service.Domain;

/// <summary>
/// Represents a hero or villain team used to demonstrate shared services and navigation mappings.
/// </summary>
public sealed class Team : BaseEntity
{
    /// <summary>
    /// Gets or sets the team name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main city where the team operates.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the headquarters label shown by API responses.
    /// </summary>
    public string Headquarters { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the calculated reputation score maintained by a cron job.
    /// </summary>
    public int Reputation { get; set; }

    /// <summary>
    /// Gets or sets the heroes that belong to this team.
    /// </summary>
    public ICollection<Hero> Heroes { get; set; } = [];

    /// <summary>
    /// Gets or sets the villains that belong to this team.
    /// </summary>
    public ICollection<Villain> Villains { get; set; } = [];
}
