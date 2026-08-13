using Heroes.Service.Business.Heroes.Models.Responses;
using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Requests;

namespace Heroes.Service.Business.Heroes.Models.Requests;

public sealed class UpdateHeroRequest : BaseRequest, IRequest<HeroResponse>
{
    /// <summary>
    /// Gets or sets the public codename used by the character.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character civilian identity.
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city associated with the resource.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative power level used by demo business rules.
    /// </summary>
    public int PowerLevel { get; set; }

    /// <summary>
    /// Gets or sets the team identifier associated with the resource.
    /// </summary>
    public CId TeamId { get; set; }
}
