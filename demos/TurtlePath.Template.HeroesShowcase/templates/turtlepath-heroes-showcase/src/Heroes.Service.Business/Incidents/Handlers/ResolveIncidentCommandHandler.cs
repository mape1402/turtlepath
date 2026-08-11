using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Services.Audit;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Pelican.Mediator;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Exceptions;
using TurtlePath.Mapping;

namespace Heroes.Service.Business.Incidents.Handlers;

/// <summary>
/// Demonstrates a fully custom command handler with direct persistence and service usage.
/// </summary>
public sealed class ResolveIncidentCommandHandler : IRequestHandler<ResolveIncidentRequest, IncidentResponse>
{
    private readonly IDbContext _dbContext;
    private readonly IMapperAdapter _mapper;
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolveIncidentCommandHandler"/> class.
    /// </summary>
    public ResolveIncidentCommandHandler(IDbContext _dbContext, IMapperAdapter _mapper, IAuditTrail _auditTrail)
    {
        this._dbContext = _dbContext;
        this._mapper = _mapper;
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public async Task<IncidentResponse> Handle(ResolveIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _dbContext.Set<Domain.Incident>().FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Incident), request.Id.ToString());

        if (incident.AssignedHeroId is null)
            throw new InvalidOperationException("Assign the incident before resolving it.");

        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _auditTrail.Add($"Incident '{incident.Title}' resolved. Notes: {request.ResolutionNotes}");

        return await _mapper.MapAsync<Domain.Incident, IncidentResponse>(incident, cancellationToken);
    }
}
