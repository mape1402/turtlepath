namespace TurtlePath.Automations.AspNetCore;

using Microsoft.AspNetCore.Http;
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
        string name = null,
        Func<HttpContext, CancellationToken, ValueTask<TRequest>> requestFactory = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationEndpointMetadata.Endpoint,
            new AutomationEndpointOptions(route, method, name, WrapRequestFactory(requestFactory)));

        return builder;
    }

    /// <summary>
    /// Exposes a mutation automation request as an ASP.NET Core endpoint.
    /// </summary>
    public static IMutationAutomationBuilder<TRequest, TEntity, TKey> Endpoint<TRequest, TEntity, TKey>(
        this IMutationAutomationBuilder<TRequest, TEntity, TKey> builder,
        string route,
        Func<HttpContext, TRequest> requestFactory,
        string method = null,
        string name = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(requestFactory);

        return builder.Endpoint(
            route,
            method,
            name,
            (context, _) => ValueTask.FromResult(requestFactory(context)));
    }

    /// <summary>
    /// Exposes a query automation request as an ASP.NET Core endpoint.
    /// </summary>
    public static IQueryAutomationBuilder<TQuery, TEntity, TKey> Endpoint<TQuery, TEntity, TKey>(
        this IQueryAutomationBuilder<TQuery, TEntity, TKey> builder,
        string route,
        string method = null,
        string name = null,
        Func<HttpContext, CancellationToken, ValueTask<TQuery>> requestFactory = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.SetMetadata(
            AutomationEndpointMetadata.Endpoint,
            new AutomationEndpointOptions(route, method, name, WrapRequestFactory(requestFactory)));

        return builder;
    }

    /// <summary>
    /// Exposes a query automation request as an ASP.NET Core endpoint.
    /// </summary>
    public static IQueryAutomationBuilder<TQuery, TEntity, TKey> Endpoint<TQuery, TEntity, TKey>(
        this IQueryAutomationBuilder<TQuery, TEntity, TKey> builder,
        string route,
        Func<HttpContext, TQuery> requestFactory,
        string method = null,
        string name = null)
        where TEntity : class, IEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(requestFactory);

        return builder.Endpoint(
            route,
            method,
            name,
            (context, _) => ValueTask.FromResult(requestFactory(context)));
    }

    private static Func<HttpContext, CancellationToken, ValueTask<object>> WrapRequestFactory<TRequest>(
        Func<HttpContext, CancellationToken, ValueTask<TRequest>> requestFactory)
        => requestFactory is null
            ? null
            : async (context, cancellationToken) => await requestFactory(context, cancellationToken);
}
