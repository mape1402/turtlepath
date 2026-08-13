using DataScorpio.Profiles;
using Heroes.Service.Domain;

namespace Heroes.Service.Business.Heroes.Querying;

/// <summary>
/// Shows DataScorpio aliases, default sort and custom filters for hero paging endpoints.
/// </summary>
public sealed class HeroQueryProfile : QueryProfile<Hero>
{
    /// <inheritdoc />
    public override void Configure(IQueryProfileBuilder<Hero> builder)
    {
        builder
            .AllowFilter("alias", hero => hero.Alias)
            .AllowFilter("city", hero => hero.City)
            .AllowFilter("active", hero => hero.Active)
            .AllowFilter("team", hero => hero.TeamId)
            .AllowSort("alias", hero => hero.Alias)
            .AllowSort("power", hero => hero.PowerLevel)
            .AllowSearch(hero => hero.Alias)
            .AllowSearch(hero => hero.RealName)
            .DefaultSort(hero => hero.Alias)
            .CustomFilter("Elite", (query, value) =>
            {
                var enabled = Convert.ToBoolean(value.Value);
                return enabled ? query.Where(hero => hero.PowerLevel >= 85) : query;
            });
    }
}
