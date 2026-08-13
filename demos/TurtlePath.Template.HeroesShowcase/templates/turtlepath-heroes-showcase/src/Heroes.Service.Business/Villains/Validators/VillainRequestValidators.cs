using Crabalidator;
using Heroes.Service.Business.Villains.Models.Requests;

namespace Heroes.Service.Business.Villains.Validators;

public sealed class CreateVillainRequestValidator : CrabValidator<CreateVillainRequest>
{
    public CreateVillainRequestValidator()
    {
        RuleFor(request => request.Alias).NotEmpty().MaximumLength(120);
        RuleFor(request => request.RealName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Lair).NotEmpty().MaximumLength(160);
        RuleFor(request => request.PowerLevel).InclusiveBetween(1, 100);
        RuleFor(request => request.TeamId).Must(id => !id.IsEmpty);
    }
}

public sealed class UpdateVillainRequestValidator : CrabValidator<UpdateVillainRequest>
{
    public UpdateVillainRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
        RuleFor(request => request.Alias).NotEmpty().MaximumLength(120);
        RuleFor(request => request.RealName).NotEmpty().MaximumLength(160);
        RuleFor(request => request.Lair).NotEmpty().MaximumLength(160);
        RuleFor(request => request.PowerLevel).InclusiveBetween(1, 100);
        RuleFor(request => request.TeamId).Must(id => !id.IsEmpty);
    }
}

public sealed class CaptureVillainRequestValidator : CrabValidator<CaptureVillainRequest>
{
    public CaptureVillainRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
    }
}
