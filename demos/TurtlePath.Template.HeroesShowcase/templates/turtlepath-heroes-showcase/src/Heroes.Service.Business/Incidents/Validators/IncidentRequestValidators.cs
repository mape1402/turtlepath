using Crabalidator;
using Heroes.Service.Business.Incidents.Models.Requests;

namespace Heroes.Service.Business.Incidents.Validators;

public sealed class ReportIncidentRequestValidator : CrabValidator<ReportIncidentRequest>
{
    public ReportIncidentRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(180);
        RuleFor(request => request.City).NotEmpty().MaximumLength(80);
    }
}

public sealed class AssignIncidentRequestValidator : CrabValidator<AssignIncidentRequest>
{
    public AssignIncidentRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
        RuleFor(request => request.HeroId).Must(id => !id.IsEmpty);
    }
}

public sealed class ResolveIncidentRequestValidator : CrabValidator<ResolveIncidentRequest>
{
    public ResolveIncidentRequestValidator()
    {
        RuleFor(request => request.Id).Must(id => !id.IsEmpty);
        RuleFor(request => request.ResolutionNotes).NotEmpty().MaximumLength(500);
    }
}
