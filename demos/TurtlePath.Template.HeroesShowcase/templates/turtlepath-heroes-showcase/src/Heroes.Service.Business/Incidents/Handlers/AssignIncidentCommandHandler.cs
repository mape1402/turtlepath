using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Incidents.Services.Workflow;
using Pelican.Mediator;
using TurtlePath.Mapping;

namespace Heroes.Service.Business.Incidents.Handlers;

/// <summary>
/// Demonstrates a fully custom Pelican handler for a workflow that does not fit a generated automation path.
/// </summary>
public sealed class AssignIncidentCommandHandler : IRequestHandler<AssignIncidentRequest, IncidentResponse>
{
    private readonly IIncidentWorkflowService _incidentWorkflowService;
    private readonly IMapperAdapter _mapper;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignIncidentCommandHandler"/> class.
    /// </summary>
    public AssignIncidentCommandHandler(
        IIncidentWorkflowService _incidentWorkflowService,
        IMapperAdapter _mapper,
        IAuditTrail _auditTrail)
    {
        this._incidentWorkflowService = _incidentWorkflowService;
        this._mapper = _mapper;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public async Task<IncidentResponse> Handle(AssignIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentWorkflowService.AssignAsync(request, cancellationToken);
        _auditTrail.Add($"Incident '{incident.Title}' assigned to hero '{request.HeroId}'.");

        return await _mapper.MapAsync<Domain.Incident, IncidentResponse>(incident, cancellationToken);
    }
}
