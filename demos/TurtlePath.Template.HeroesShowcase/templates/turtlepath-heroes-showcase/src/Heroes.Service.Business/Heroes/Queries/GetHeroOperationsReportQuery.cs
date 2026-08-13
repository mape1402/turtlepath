using Heroes.Service.Business.Heroes.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Heroes.Queries;

/// <summary>
/// Query that demonstrates bypassing the standard EF query path for a specialized read model.
/// </summary>
public sealed class GetHeroOperationsReportQuery : IRequest<HeroOperationsReportResponse>
{
    /// <summary>
    /// Gets or sets the optional team filter.
    /// </summary>
    public CId? TeamId { get; set; }
}
