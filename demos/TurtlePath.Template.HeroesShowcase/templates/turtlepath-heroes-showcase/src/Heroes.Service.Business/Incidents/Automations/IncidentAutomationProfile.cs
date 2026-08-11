using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Incidents.Queries;
using Heroes.Service.Domain;
using TurtlePath.Automations.Profiles;

namespace Heroes.Service.Business.Incidents.Automations;

/// <summary>
/// Uses automation for reporting and reading incidents, while assignment and resolution use custom handlers.
/// </summary>
public sealed class IncidentAutomationProfile : TurtlePathAutomationProfile
{
    /// <inheritdoc />
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Incident>()
            .ToCreate<ReportIncidentRequest, IncidentResponse>()
            .ToGetById<GetIncidentByIdQuery, IncidentResponse>()
            .ToGetPaged<GetPagedIncidentsQuery, IncidentResponse>(query => query.DefaultSort("-threat"));
    }
}
