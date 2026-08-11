using TurtlePath.Domain.Contracts;

namespace Heroes.Service.Domain;

/// <summary>
/// Demonstrates a legacy table whose database key is an integer while application code still works with <c>CId</c>.
/// </summary>
public sealed class LegacyCaseFile : BaseEntity
{
    /// <summary>
    /// Gets or sets the external agency case number.
    /// </summary>
    public string ExternalNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city where the legacy file was opened.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the file has already been imported into the incident workflow.
    /// </summary>
    public bool Imported { get; set; }
}
