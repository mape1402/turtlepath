using Crabalidator;
using Heroes.Service.Business.Heroes.Models.Requests;

namespace Heroes.Service.Business.Heroes.Validators;

public sealed class CreateHeroRequestValidator : CrabValidator<CreateHeroRequest>
{
    public CreateHeroRequestValidator()
    {
        RuleFor(request => request.Alias).NotEmpty().MaximumLength(120);
        RuleFor(request => request.RealName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.City).NotEmpty().MaximumLength(80);
        RuleFor(request => request.PowerLevel).InclusiveBetween(1, 100);
        RuleFor(request => request.TeamId).Must(id => !id.IsEmpty);
    }
}

public sealed class UpdateHeroRequestValidator : CrabValidator<UpdateHeroRequest>
{
    public UpdateHeroRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
        RuleFor(request => request.Alias).NotEmpty().MaximumLength(120);
        RuleFor(request => request.RealName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.City).NotEmpty().MaximumLength(80);
        RuleFor(request => request.PowerLevel).InclusiveBetween(1, 100);
        RuleFor(request => request.TeamId).Must(id => !id.IsEmpty);
    }
}

public sealed class DeactivateHeroRequestValidator : CrabValidator<DeactivateHeroRequest>
{
    public DeactivateHeroRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
    }
}
