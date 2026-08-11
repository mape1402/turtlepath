using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Domain;
using OctoMap;

namespace Heroes.Service.Business.Incidents.Mappings;

public sealed class IncidentMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<ReportIncidentRequest, Incident>();
        builder.CreateMap<Incident, IncidentResponse>();
    }
}
