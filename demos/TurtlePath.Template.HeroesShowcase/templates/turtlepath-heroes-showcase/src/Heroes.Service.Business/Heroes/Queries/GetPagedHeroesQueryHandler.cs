using System.Linq.Expressions;
using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Heroes.Queries;

/// <summary>
/// Demonstrates a paged query override with mandatory internal filters plus public DataScorpio filters.
/// </summary>
public sealed class GetPagedHeroesQueryHandler : GenericGetPagedInfoQueryHandler<GetPagedHeroesQuery, Hero, HeroResponse, CId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPagedHeroesQueryHandler"/> class.
    /// </summary>
    public GetPagedHeroesQueryHandler(IServiceProvider services) : base(services)
    {
    }

    /// <inheritdoc />
    protected override string DefaultSorts => "alias";

    /// <inheritdoc />
    protected override Expression<Func<Hero, object>>[] GetIncludeExpressions(GetPagedHeroesQuery request)
        => [hero => hero.Team];

    /// <inheritdoc />
    protected override Expression<Func<Hero, bool>> GetFiltersExpression(GetPagedHeroesQuery query)
    {
        if (query.TeamId is null)
            return hero => hero.Active;

        var teamId = query.TeamId.Value;
        return hero => hero.Active && hero.TeamId == teamId;
    }
}
