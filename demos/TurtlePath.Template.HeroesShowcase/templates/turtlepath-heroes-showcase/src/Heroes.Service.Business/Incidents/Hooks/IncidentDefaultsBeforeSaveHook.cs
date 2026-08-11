using Heroes.Service.Business.Incidents.Models.Requests;
using Heroes.Service.Domain.Enums;
using TurtlePath.Hooks;

namespace Heroes.Service.Business.Incidents.Hooks;

/// <summary>
/// Demonstrates a request-specific hook that sets incident defaults before persistence.
/// </summary>
public sealed class IncidentDefaultsBeforeSaveHook : IBeforeSaveHook<ReportIncidentRequest, Domain.Incident>
{
    /// <inheritdoc />
    public ValueTask BeforeSaveAsync(CommandHookContext<ReportIncidentRequest, Domain.Incident> context, CancellationToken cancellationToken = default)
    {
        context.Entity.Status = IncidentStatus.Reported;
        context.Entity.ReportedAt = DateTimeOffset.UtcNow;
        return ValueTask.CompletedTask;
    }
}
