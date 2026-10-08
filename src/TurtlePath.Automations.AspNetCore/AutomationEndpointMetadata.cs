namespace TurtlePath.Automations.AspNetCore;

/// <summary>
/// Defines metadata keys used by the TurtlePath automation ASP.NET Core integration.
/// </summary>
public static class AutomationEndpointMetadata
{
    /// <summary>
    /// Metadata key that stores <see cref="AutomationEndpointOptions"/>.
    /// </summary>
    public const string Endpoint = "TurtlePath.Automations.AspNetCore.Endpoint";

    /// <summary>
    /// Metadata key that stores <see cref="AutomationEndpointDefaults"/>.
    /// </summary>
    public const string EndpointDefaults = "TurtlePath.Automations.AspNetCore.EndpointDefaults";
}
