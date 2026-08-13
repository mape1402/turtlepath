using DataScorpio.Profiles;
using DataScorpio.Querying;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;

namespace Heroes.Service.Business.Villains.Querying;

/// <summary>
/// Shows custom business filters and custom sort semantics for villains.
/// </summary>
public sealed class VillainQueryProfile : QueryProfile<Villain>
{
    /// <inheritdoc />
    public override void Configure(IQueryProfileBuilder<Villain> builder)
    {
        builder
            .AllowFilter("alias", villain => villain.Alias)
            .AllowFilter("threat", villain => villain.ThreatLevel)
            .AllowFilter("captured", villain => villain.Captured)
            .AllowSort("alias", villain => villain.Alias)
            .AllowSort("power", villain => villain.PowerLevel)
            .AllowSearch(villain => villain.Alias)
            .CustomFilter("AtLarge", (query, value) =>
            {
                var enabled = Convert.ToBoolean(value.Value);
                return enabled ? query.Where(villain => !villain.Captured) : query;
            })
            .CustomSort("Danger", (query, direction) =>
                direction == SortDirection.Descending
                    ? query.OrderByDescending(villain => villain.ThreatLevel == ThreatLevel.Critical).ThenByDescending(villain => villain.PowerLevel)
                    : query.OrderBy(villain => villain.ThreatLevel == ThreatLevel.Critical).ThenBy(villain => villain.PowerLevel));
    }
}
