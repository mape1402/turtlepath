using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Business.Services.Incident;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Pelican.Mediator;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Exceptions;
using TurtlePath.Mapping;

namespace Heroes.Service.Business.Incidents.Handlers;

/// <summary>
/// Demonstrates a fully custom Pelican handler for a workflow that does not fit a generated automation path.
/// </summary>
public sealed class AssignIncidentCommandHandler : IRequestHandler<AssignIncidentRequest, IncidentResponse>
{
    private readonly IDbContext _dbContext;
    private readonly IIncidentAssignmentService _assignmentService;
    private readonly IMapperAdapter _mapper;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssignIncidentCommandHandler"/> class.
    /// </summary>
    public AssignIncidentCommandHandler(
        IDbContext _dbContext,
        IIncidentAssignmentService _assignmentService,
        IMapperAdapter _mapper,
        IAuditTrail _auditTrail)
    {
        this._dbContext = _dbContext;
        this._assignmentService = _assignmentService;
        this._mapper = _mapper;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public async Task<IncidentResponse> Handle(AssignIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _dbContext.Set<Domain.Incident>().FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Incident), request.Id.ToString());

        if (incident.Status == IncidentStatus.Resolved)
            throw new InvalidOperationException("Resolved incidents cannot be reassigned.");

        await _assignmentService.AssignAsync(incident, request.HeroId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _auditTrail.Add($"Incident '{incident.Title}' assigned to hero '{request.HeroId}'.");

        return await _mapper.MapAsync<Domain.Incident, IncidentResponse>(incident, cancellationToken);
    }
}
