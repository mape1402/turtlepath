using DataScorpio.Profiles;
using DataScorpio.Querying;
using Heroes.Service.Domain;
using Heroes.Service.Domain.Enums;

namespace Heroes.Service.Business.Incidents.Querying;

/// <summary>
/// Demonstrates query aliases, max page size and custom incident filters.
/// </summary>
public sealed class IncidentQueryProfile : QueryProfile<Incident>
{
    /// <inheritdoc />
    public override void Configure(IQueryProfileBuilder<Incident> builder)
    {
        builder
            .AllowFilter("city", incident => incident.City)
            .AllowFilter("status", incident => incident.Status)
            .AllowFilter("threat", incident => incident.ThreatLevel)
            .AllowSort("city", incident => incident.City)
            .AllowSort("threat", incident => incident.ThreatLevel)
            .AllowSearch(incident => incident.Title)
            .DefaultSort(incident => incident.ThreatLevel, SortDirection.Descending)
            .MaxPageSize(100)
            .CustomFilter("Open", (query, value) =>
            {
                var enabled = Convert.ToBoolean(value.Value);
                return enabled
                    ? query.Where(incident => incident.Status != IncidentStatus.Resolved && incident.Status != IncidentStatus.Archived)
                    : query;
            });
    }
}
