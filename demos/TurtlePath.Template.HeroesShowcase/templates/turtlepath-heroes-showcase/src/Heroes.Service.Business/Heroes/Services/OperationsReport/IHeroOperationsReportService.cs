using Heroes.Service.Business.Heroes.Models.Responses;
using TurtlePath.Domain.Identifier;

namespace Heroes.Service.Business.Heroes.Services.OperationsReport;

/// <summary>
/// Builds the hero operations report used by dashboards or operations endpoints.
/// </summary>
public interface IHeroOperationsReportService
{
    /// <summary>
    /// Builds a compact operations report for active heroes.
    /// </summary>
    /// <param name="teamId">Optional team filter.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>The operations report.</returns>
    Task<HeroOperationsReportResponse> GetOperationsReportAsync(CId? teamId, CancellationToken cancellationToken = default);
}
