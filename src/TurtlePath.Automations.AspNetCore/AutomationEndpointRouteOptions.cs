namespace TurtlePath.Automations.AspNetCore;

/// <summary>
/// Configures route mapping for automation endpoints.
/// </summary>
public sealed class AutomationEndpointRouteOptions
{
    /// <summary>
    /// Gets or sets the route prefix prepended to every automation endpoint.
    /// </summary>
    public string RoutePrefix { get; set; } = string.Empty;
}
