using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Business.Heroes.Queries;
using Heroes.Service.Business.Incidents.Models.Responses;
using Heroes.Service.Business.Incidents.Queries;
using TurtlePath.Hooks;
using TurtlePath.Models.Responses;

namespace Heroes.Service.Business.Services.Audit;

/// <summary>
/// Demonstrates query hooks for metrics/audit around read operations.
/// </summary>
public sealed class QueryAuditHook :
    IAfterQueryHook<GetPagedHeroesQuery, PagedResponse<HeroResponse>>,
    IAfterQueryHook<GetPagedIncidentsQuery, PagedResponse<IncidentResponse>>
{
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryAuditHook"/> class.
    /// </summary>
    public QueryAuditHook(IAuditTrail _auditTrail)
    {
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public ValueTask AfterQueryAsync(QueryHookContext<GetPagedHeroesQuery, PagedResponse<HeroResponse>> context, CancellationToken cancellationToken = default)
    {
        _auditTrail.Add($"Paged heroes queried: {context.Result?.RowCount ?? 0} total rows.");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask AfterQueryAsync(QueryHookContext<GetPagedIncidentsQuery, PagedResponse<IncidentResponse>> context, CancellationToken cancellationToken = default)
    {
        _auditTrail.Add($"Paged incidents queried: {context.Result?.RowCount ?? 0} total rows.");
        return ValueTask.CompletedTask;
    }
}
