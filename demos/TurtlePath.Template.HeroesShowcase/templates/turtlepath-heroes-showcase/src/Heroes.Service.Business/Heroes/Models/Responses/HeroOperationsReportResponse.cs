namespace Heroes.Service.Business.Heroes.Models.Responses;

/// <summary>
/// Response returned by the raw ADO.NET hero operations read model.
/// </summary>
public sealed class HeroOperationsReportResponse
{
    /// <summary>
    /// Gets or sets the number of active heroes in the report.
    /// </summary>
    public int ActiveHeroes { get; set; }

    /// <summary>
    /// Gets or sets the total open assignments across reported heroes.
    /// </summary>
    public int OpenAssignments { get; set; }

    /// <summary>
    /// Gets or sets the rows returned by the operations read model.
    /// </summary>
    public IReadOnlyList<HeroOperationsRowResponse> Heroes { get; set; } = [];
}
