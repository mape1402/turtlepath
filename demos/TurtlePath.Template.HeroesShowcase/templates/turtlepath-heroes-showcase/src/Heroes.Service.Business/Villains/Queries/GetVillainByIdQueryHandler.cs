using Heroes.Service.Business.Villains.Models.Responses;
using Heroes.Service.Domain;
using System.Linq.Expressions;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Villains.Queries;

/// <summary>
/// Reads one villain with its team so relationship data can be mapped safely.
/// </summary>
public sealed class GetVillainByIdQueryHandler : GenericGetByIdQueryHandler<GetVillainByIdQuery, Villain, VillainResponse, CId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetVillainByIdQueryHandler"/> class.
    /// </summary>
    public GetVillainByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc />
    protected override Expression<Func<Villain, object>>[] GetIncludeExpressions(GetVillainByIdQuery request)
        => [villain => villain.Team];
}
