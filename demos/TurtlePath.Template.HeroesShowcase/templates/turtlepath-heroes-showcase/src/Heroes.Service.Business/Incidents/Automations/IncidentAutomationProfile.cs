using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Incidents.Queries;
using Heroes.Service.Domain;
using TurtlePath.Automations.AspNetCore;
using TurtlePath.Automations.Pigeon;
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
            .ToCreate<ReportIncidentRequest, IncidentResponse>(operation => operation
                .Endpoint("automation/incidents/report", name: "ReportIncidentAutomation")
                .Consume("heroes.incidents.report", "1.0.0", "heroes-automation"))
            .ToGetById<GetIncidentByIdQuery, IncidentResponse>(operation => operation
                .Endpoint("automation/incidents/{id}", name: "GetIncidentAutomation"))
            .ToGetPaged<GetPagedIncidentsQuery, IncidentResponse>(query => query.DefaultSort("-threat"));
    }
}
