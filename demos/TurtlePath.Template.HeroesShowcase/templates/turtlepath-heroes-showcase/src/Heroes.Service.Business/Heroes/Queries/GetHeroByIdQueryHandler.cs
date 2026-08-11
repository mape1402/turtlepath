using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Domain;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Heroes.Queries;

/// <summary>
/// Reads one hero with its team so the response can expose relationship data.
/// </summary>
public sealed class GetHeroByIdQueryHandler : GenericGetByIdQueryHandler<GetHeroByIdQuery, Hero, HeroResponse, CId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetHeroByIdQueryHandler"/> class.
    /// </summary>
    public GetHeroByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
