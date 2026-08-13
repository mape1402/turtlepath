using Heroes.Service.Business.Heroes.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Heroes.Models.Requests;

public sealed class UpdateHeroRequest : BaseRequest, IRequest<HeroResponse>
{
    public string Alias { get; set; } = string.Empty;

    public string RealName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int PowerLevel { get; set; }

    public CId TeamId { get; set; }
}
