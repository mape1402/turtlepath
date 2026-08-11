using Heroes.Service.Business.Skills.Models.Requests;
using Heroes.Service.Business.Skills.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using TurtlePath.Spider;

namespace Heroes.Service.Api.Controllers;

/// <summary>
/// REST endpoints for hero and villain skills.
/// </summary>
[Route("skills")]
public sealed class SkillsController : BaseController
{
    /// <summary>
    /// Adds a skill to a hero.
    /// </summary>
    [HttpPost("hero")]
    public Task<SkillResponse> CreateHeroSkill([FromBody] CreateHeroSkillRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<CreateHeroSkillRequest, SkillResponse>(request, cancellationToken);

    /// <summary>
    /// Adds a skill to a villain.
    /// </summary>
    [HttpPost("villain")]
    public Task<SkillResponse> CreateVillainSkill([FromBody] CreateVillainSkillRequest request, CancellationToken cancellationToken)
        => Spider.DefaultSend<CreateVillainSkillRequest, SkillResponse>(request, cancellationToken);
}
