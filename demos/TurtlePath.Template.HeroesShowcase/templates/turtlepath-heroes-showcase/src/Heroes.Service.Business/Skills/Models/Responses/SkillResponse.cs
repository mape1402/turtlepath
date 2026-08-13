using Heroes.Service.Domain.Enums;
using TurtlePath.Domain.Identifier;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Skills.Models.Responses;

public sealed class SkillResponse : BaseResponse
{
    public string Name { get; set; } = string.Empty;

    public Alignment OwnerAlignment { get; set; }

    public int Mastery { get; set; }

    public CId? HeroId { get; set; }

    public CId? VillainId { get; set; }
}
