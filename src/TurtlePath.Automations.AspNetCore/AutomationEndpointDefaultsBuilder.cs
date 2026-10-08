namespace TurtlePath.Automations.AspNetCore;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Configures ASP.NET Core endpoint defaults inherited by automation endpoints for one entity.
/// </summary>
public sealed class AutomationEndpointDefaultsBuilder
{
    private readonly List<object> metadata = [];

    /// <summary>
    /// Adds endpoint metadata inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder Metadata(params object[] metadata)
    {
        if (metadata is null)
            throw new ArgumentNullException(nameof(metadata));

        this.metadata.AddRange(metadata.Where(item => item is not null));
        return this;
    }

    /// <summary>
    /// Requires authorization for every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder Authorize(params string[] policies)
    {
        if (policies is null || policies.Length == 0)
        {
            metadata.Add(new AuthorizeAttribute());
            return this;
        }

        foreach (var policy in policies.Where(item => !string.IsNullOrWhiteSpace(item)))
            metadata.Add(new AuthorizeAttribute(policy));

        return this;
    }

    /// <summary>
    /// Allows anonymous access for every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder AllowAnonymous()
    {
        metadata.Add(new AllowAnonymousAttribute());
        return this;
    }

    internal AutomationEndpointDefaults Build()
        => new(metadata);
}
