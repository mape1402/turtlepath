using Heroes.Service.Business.Skills.Models.Responses;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;
using TurtlePath.Queries;

namespace Heroes.Service.Business.Skills.Queries;

public sealed class GetSkillsQuery : GetManyQuery<Skill, SkillResponse>
{
    public Alignment? OwnerAlignment { get; init; }
}
