using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using System.Linq.Expressions;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Villains.Queries;

/// <summary>
/// Reads villains through TurtlePath paging while including team data for response mapping.
/// </summary>
public sealed class GetPagedVillainsQueryHandler : GenericGetPagedInfoQueryHandler<GetPagedVillainsQuery, Villain, VillainResponse, CId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPagedVillainsQueryHandler"/> class.
    /// </summary>
    public GetPagedVillainsQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    protected override string DefaultSorts => "-power";

    /// <inheritdoc />
    protected override Expression<Func<Villain, object>>[] GetIncludeExpressions(GetPagedVillainsQuery request)
        => [villain => villain.Team];
}
