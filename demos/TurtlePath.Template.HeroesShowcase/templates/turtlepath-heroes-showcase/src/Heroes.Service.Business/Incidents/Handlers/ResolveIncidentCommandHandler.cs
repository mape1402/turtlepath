using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Incidents.Services.Workflow;
using Pelican.Mediator;
using TurtlePath.Mapping;

namespace Heroes.Service.Business.Incidents.Handlers;

/// <summary>
/// Demonstrates a fully custom command handler that keeps persistence details inside a feature service.
/// </summary>
public sealed class ResolveIncidentCommandHandler : IRequestHandler<ResolveIncidentRequest, IncidentResponse>
{
    private readonly IIncidentWorkflowService _incidentWorkflowService;
    private readonly IMapperAdapter _mapper;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveIncidentCommandHandler"/> class.
    /// </summary>
    public ResolveIncidentCommandHandler(IIncidentWorkflowService _incidentWorkflowService, IMapperAdapter _mapper, IAuditTrail _auditTrail)
    {
        this._incidentWorkflowService = _incidentWorkflowService;
        this._mapper = _mapper;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public async Task<IncidentResponse> Handle(ResolveIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentWorkflowService.ResolveAsync(request, cancellationToken);
        _auditTrail.Add($"Incident '{incident.Title}' resolved. Notes: {request.ResolutionNotes}");

        return await _mapper.MapAsync<Domain.Incident, IncidentResponse>(incident, cancellationToken);
    }
}
