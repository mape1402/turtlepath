using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Business.Heroes.Services.OperationsReport;
using Pelican.Mediator;

namespace Heroes.Service.Business.Heroes.Queries;

/// <summary>
/// Thin handler that delegates a non-standard ADO.NET read model to a feature service.
/// </summary>
public sealed class GetHeroOperationsReportQueryHandler : IRequestHandler<GetHeroOperationsReportQuery, HeroOperationsReportResponse>
{
    private readonly IHeroOperationsReportService _reportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetHeroOperationsReportQueryHandler"/> class.
    /// </summary>
    /// <param name="reportService">The feature service that builds the operations report.</param>
    public GetHeroOperationsReportQueryHandler(IHeroOperationsReportService reportService)
    {
        _reportService = reportService;
    }

    /// <inheritdoc />
    public Task<HeroOperationsReportResponse> Handle(GetHeroOperationsReportQuery request, CancellationToken cancellationToken = default)
        => _reportService.GetOperationsReportAsync(request.TeamId, cancellationToken);
}
