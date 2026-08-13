using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Incidents.Services.Assignment;
using Heroes.Service.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TurtlePath.EntityFrameworkCore;
using TurtlePath.Exceptions;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Incidents.Services.Workflow;

/// <summary>
/// EF-backed incident workflow service used by custom handlers.
/// </summary>
public sealed class IncidentWorkflowService : IIncidentWorkflowService
{
    private readonly IDbContext _dbContext;
    private readonly IIncidentAssignmentService _assignmentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentWorkflowService"/> class.
    /// </summary>
    /// <param name="dbContext">The application database context abstraction.</param>
    /// <param name="assignmentService">The assignment domain service.</param>
    public IncidentWorkflowService(IDbContext dbContext, IIncidentAssignmentService assignmentService)
    {
        _dbContext = dbContext;
        _assignmentService = assignmentService;
    }

    /// <inheritdoc />
    public async Task<IncidentEntity> AssignAsync(AssignIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _dbContext.Set<IncidentEntity>().FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IncidentEntity), request.Id.ToString());

        if (incident.Status == IncidentStatus.Resolved)
            throw new InvalidOperationException("Resolved incidents cannot be reassigned.");

        await _assignmentService.AssignAsync(incident, request.HeroId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return incident;
    }

    /// <inheritdoc />
    public async Task<IncidentEntity> ResolveAsync(ResolveIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _dbContext.Set<IncidentEntity>().FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(IncidentEntity), request.Id.ToString());

        if (incident.AssignedHeroId is null)
            throw new InvalidOperationException("Assign the incident before resolving it.");

        incident.Status = IncidentStatus.Resolved;
        incident.ResolvedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return incident;
    }
}
