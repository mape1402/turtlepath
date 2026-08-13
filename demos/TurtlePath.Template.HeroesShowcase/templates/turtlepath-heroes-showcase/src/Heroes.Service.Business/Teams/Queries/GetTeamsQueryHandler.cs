using System.Linq.Expressions;
using Heroes.Service.Business.Teams.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Teams.Queries;

/// <summary>
/// Demonstrates a custom get-many query handler that still reuses TurtlePath storage, hooks and mapping.
/// </summary>
public sealed class GetTeamsQueryHandler : GetManyQueryHandler<GetTeamsQuery, Team, TeamResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetTeamsQueryHandler"/> class.
    /// </summary>
    public GetTeamsQueryHandler(IServiceProvider services) : base(services)
    {
    }

    /// <inheritdoc />
    protected override Expression<Func<Team, bool>> GetFilterExpression(GetTeamsQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.City))
            return team => true;

        var city = query.City.Trim().ToLowerInvariant();
        return team => team.City.ToLower() == city;
    }

    /// <inheritdoc />
    protected override Expression<Func<Team, object>> GetSortingExpression(GetTeamsQuery query)
        => team => team.Name;
}
