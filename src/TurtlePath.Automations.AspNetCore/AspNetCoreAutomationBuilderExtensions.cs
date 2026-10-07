namespace TurtlePath.Automations.AspNetCore;

using TurtlePath.Automations.Profiles;
using TurtlePath.Domain.Contracts;

/// <summary>
/// Provides ASP.NET Core endpoint metadata extensions for TurtlePath automation profiles.
/// </summary>
public static class AspNetCoreAutomationBuilderExtensions
{
    /// <summary>
    /// Exposes a mutation automation request as an ASP.NET Core endpoint.
    /// </summary>
    public static IMutationAutomationBuilder<TRequest, TEntity, TKey> Endpoint<TRequest, TEntity, TKey>(
        this IMutationAutomationBuilder<TRequest, TEntity, TKey> builder,
        string route,
        string method = null,
        string name = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationEndpointMetadata.Endpoint,
            new AutomationEndpointOptions(route, method, name));

        return builder;
    }

    /// <summary>
    /// Exposes a query automation request as an ASP.NET Core endpoint.
    /// </summary>
    public static IQueryAutomationBuilder<TQuery, TEntity, TKey> Endpoint<TQuery, TEntity, TKey>(
        this IQueryAutomationBuilder<TQuery, TEntity, TKey> builder,
        string route,
        string method = null,
        string name = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationEndpointMetadata.Endpoint,
            new AutomationEndpointOptions(route, method, name));

        return builder;
    }
}
