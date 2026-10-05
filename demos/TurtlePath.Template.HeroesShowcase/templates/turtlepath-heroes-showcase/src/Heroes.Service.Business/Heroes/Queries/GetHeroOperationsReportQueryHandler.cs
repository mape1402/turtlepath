using Heroes.Service.Business.Heroes.Models.Responses;
using Heroes.Service.Business.Heroes.Services.OperationsReport;
using Pelican.Mediator;
using Spider.Pipelines.Core;

namespace Heroes.Service.Business.Heroes.Queries;

/// <summary>
/// Thin handler that delegates a non-standard ADO.NET read model to a feature service.
/// </summary>
public sealed class GetHeroOperationsReportQueryHandler : IRequestHandler<GetHeroOperationsReportQuery, HeroOperationsReportResponse>
{
    private readonly IHeroOperationsReportService _reportService;
    private readonly ISpider _spider;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetHeroOperationsReportQueryHandler"/> class.
    /// </summary>
    /// <param name="reportService">The feature service that builds the operations report.</param>
    /// <param name="spider">The Spider runtime used to describe and trace the custom report flow.</param>
    public GetHeroOperationsReportQueryHandler(IHeroOperationsReportService reportService, ISpider spider = null)
    {
        _reportService = reportService;
        _spider = spider;
    }

    /// <inheritdoc />
    public Task<HeroOperationsReportResponse> Handle(GetHeroOperationsReportQuery request, CancellationToken cancellationToken = default)
    {
        if (_spider is null)
            return BuildReportAsync(request, cancellationToken);

        return _spider
            .ComposeFlow<GetHeroOperationsReportQuery, HeroOperationsReportResponse>("Build hero operations report")
            .Describe("Runs the Heroes demo custom ADO.NET read model and returns the aggregated operations report.")
            .Tags("heroes", "custom-handler", "report", "read-model")
            .UsingProfile("Heroes.Report")
            .Then(BuildReportAsync, step => step
                .Named("Read operations report")
                .Describe("Delegates to the feature service that composes the non-standard operations report.")
                .Tags("ado-net", "report", "read-model"))
            .RunAsync(request, cancellationToken);
    }

    private Task<HeroOperationsReportResponse> BuildReportAsync(GetHeroOperationsReportQuery request, CancellationToken cancellationToken)
        => _reportService.GetOperationsReportAsync(request.TeamId, cancellationToken);
}
