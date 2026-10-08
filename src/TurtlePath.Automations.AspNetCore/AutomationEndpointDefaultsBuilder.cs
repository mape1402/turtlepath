namespace TurtlePath.Automations.AspNetCore;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Configures ASP.NET Core endpoint defaults inherited by automation endpoints for one entity.
/// </summary>
public sealed class AutomationEndpointDefaultsBuilder
{
    private readonly List<object> metadata = [];

    /// <summary>
    /// Adds an attribute inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder UseAttribute<TAttribute>()
        where TAttribute : Attribute, new()
        => UseAttribute(new TAttribute());

    /// <summary>
    /// Adds an attribute inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder UseAttribute<TAttribute>(Func<TAttribute> attributeFactory)
        where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(attributeFactory);

        return UseAttribute(attributeFactory());
    }

    /// <summary>
    /// Adds an attribute inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder UseAttribute(Attribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        metadata.Add(attribute);
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
