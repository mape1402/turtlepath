using Crabalidator;
using Heroes.Service.Business.Teams.Models.Requests;

namespace Heroes.Service.Business.Teams.Validators;

public sealed class CreateTeamRequestValidator : CrabValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.City).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Headquarters).NotEmpty().MaximumLength(160);
    }
}

public sealed class UpdateTeamRequestValidator : CrabValidator<UpdateTeamRequest>
{
    public UpdateTeamRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.City).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Headquarters).NotEmpty().MaximumLength(160);
    }
}
