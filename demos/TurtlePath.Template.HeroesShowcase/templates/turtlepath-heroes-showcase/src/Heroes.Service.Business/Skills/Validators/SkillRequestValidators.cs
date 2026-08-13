using Crabalidator;
using Heroes.Service.Business.Skills.Models.Requests;

namespace Heroes.Service.Business.Skills.Validators;

public sealed class CreateHeroSkillRequestValidator : CrabValidator<CreateHeroSkillRequest>
{
    public CreateHeroSkillRequestValidator()
    {
        RuleFor(request => request.HeroId).Must(id => !id.IsEmpty);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Mastery).InclusiveBetween(1, 100);
    }
}

public sealed class CreateVillainSkillRequestValidator : CrabValidator<CreateVillainSkillRequest>
{
    public CreateVillainSkillRequestValidator()
    {
        RuleFor(request => request.VillainId).Must(id => !id.IsEmpty);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Mastery).InclusiveBetween(1, 100);
    }
}
