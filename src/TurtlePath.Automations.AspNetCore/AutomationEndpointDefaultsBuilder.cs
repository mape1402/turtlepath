namespace TurtlePath.Automations.AspNetCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

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
    /// Adds a response type declaration inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder ProducesResponseType(int statusCode)
    {
        metadata.Add(new ProducesResponseTypeMetadata(statusCode, null, []));
        return this;
    }

    /// <summary>
    /// Adds a response type declaration inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder ProducesResponseType<TResponse>(params string[] contentTypes)
        => ProducesResponseType(typeof(TResponse), StatusCodes.Status200OK, contentTypes);

    /// <summary>
    /// Adds a response type declaration inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder ProducesResponseType<TResponse>(int statusCode, params string[] contentTypes)
        => ProducesResponseType(typeof(TResponse), statusCode, contentTypes);

    /// <summary>
    /// Adds a response type declaration inherited by every generated endpoint for the entity.
    /// </summary>
    public AutomationEndpointDefaultsBuilder ProducesResponseType(Type responseType, int statusCode, params string[] contentTypes)
    {
        ArgumentNullException.ThrowIfNull(responseType);

        metadata.Add(new ProducesResponseTypeMetadata(
            statusCode,
            responseType,
            ResolveContentTypes(contentTypes)));

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

    private static string[] ResolveContentTypes(string[] contentTypes)
        => contentTypes is { Length: > 0 }
            ? contentTypes
            : [ "application/json" ];
}
