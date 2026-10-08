namespace TurtlePath.Automations.AspNetCore;

/// <summary>
/// Configures ASP.NET Core endpoint defaults inherited by automation endpoints for one entity.
/// </summary>
public sealed record AutomationEndpointDefaults
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutomationEndpointDefaults"/> class.
    /// </summary>
    /// <param name="metadata">The metadata applied to every generated endpoint for the entity.</param>
    public AutomationEndpointDefaults(IReadOnlyCollection<object> metadata = null)
    {
        Metadata = metadata ?? [];
    }

    /// <summary>
    /// Gets metadata applied to every generated endpoint for the entity.
    /// </summary>
    public IReadOnlyCollection<object> Metadata { get; }
}
