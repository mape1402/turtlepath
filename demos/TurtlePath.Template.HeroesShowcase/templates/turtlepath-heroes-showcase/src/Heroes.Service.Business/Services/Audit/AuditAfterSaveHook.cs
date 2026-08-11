using Heroes.Service.Business.Heroes.Models.Requests;
using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Business.Villains.Models.Requests;
using Heroes.Service.Domain;
using TurtlePath.Hooks;
using IncidentEntity = Heroes.Service.Domain.Incident;

namespace Heroes.Service.Business.Services.Audit;

/// <summary>
/// Demonstrates a cross-feature hook that records audit entries after successful saves.
/// </summary>
public sealed class AuditAfterSaveHook :
    IAfterSaveHook<CreateHeroRequest, Hero>,
    IAfterSaveHook<CreateVillainRequest, Villain>,
    IAfterSaveHook<ReportIncidentRequest, IncidentEntity>
{
    private readonly IAuditTrail _auditTrail;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditAfterSaveHook"/> class.
    /// </summary>
    public AuditAfterSaveHook(IAuditTrail _auditTrail)
    {
        this._auditTrail = _auditTrail;
    }

    /// <inheritdoc />
    public ValueTask AfterSaveAsync(CommandHookContext<CreateHeroRequest, Hero> context, CancellationToken cancellationToken = default)
    {
        _auditTrail.Add($"Hero created: {context.Entity.Alias}.");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask AfterSaveAsync(CommandHookContext<CreateVillainRequest, Villain> context, CancellationToken cancellationToken = default)
    {
        _auditTrail.Add($"Villain created: {context.Entity.Alias}.");
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask AfterSaveAsync(CommandHookContext<ReportIncidentRequest, IncidentEntity> context, CancellationToken cancellationToken = default)
    {
        _auditTrail.Add($"Incident reported: {context.Entity.Title}.");
        return ValueTask.CompletedTask;
    }
}
