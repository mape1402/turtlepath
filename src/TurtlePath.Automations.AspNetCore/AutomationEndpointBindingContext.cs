namespace TurtlePath.Automations.AspNetCore;

using System.ComponentModel;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides controlled access to endpoint values used by automation endpoint bindings.
/// </summary>
public sealed class AutomationEndpointBindingContext
{
    private readonly HttpContext context;

    internal AutomationEndpointBindingContext(HttpContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Gets a route parameter as a string.
    /// </summary>
    public string GetRouteParam(string name)
        => GetRouteParam<string>(name);

    /// <summary>
    /// Gets a route parameter converted to the requested type.
    /// </summary>
    public T GetRouteParam<T>(string name)
        => ConvertValue<T>(GetRequiredRouteValue(name));

    /// <summary>
    /// Gets a query string value as a string.
    /// </summary>
    public string GetQuery(string name)
        => GetQuery<string>(name);

    /// <summary>
    /// Gets a query string value converted to the requested type.
    /// </summary>
    public T GetQuery<T>(string name)
    {
        if (!context.Request.Query.TryGetValue(name, out var value))
            throw new BadHttpRequestException($"Required query string value '{name}' was not provided.");

        return ConvertValue<T>(value.ToString());
    }

    /// <summary>
    /// Gets a header value as a string.
    /// </summary>
    public string GetHeader(string name)
        => GetHeader<string>(name);

    /// <summary>
    /// Gets a header value converted to the requested type.
    /// </summary>
    public T GetHeader<T>(string name)
    {
        if (!context.Request.Headers.TryGetValue(name, out var value))
            throw new BadHttpRequestException($"Required header '{name}' was not provided.");

        return ConvertValue<T>(value.ToString());
    }

    private string GetRequiredRouteValue(string name)
    {
        if (!context.Request.RouteValues.TryGetValue(name, out var value) || value is null)
            throw new BadHttpRequestException($"Required route parameter '{name}' was not provided.");

        return value.ToString();
    }

    private static T ConvertValue<T>(string value)
        => (T)ConvertValue(value, typeof(T));

    private static object ConvertValue(string value, Type targetType)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        if (nullableType is not null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            targetType = nullableType;
        }

        if (targetType == typeof(string))
            return value;

        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromInvariantString(value);

        return Convert.ChangeType(value, targetType);
    }
}
