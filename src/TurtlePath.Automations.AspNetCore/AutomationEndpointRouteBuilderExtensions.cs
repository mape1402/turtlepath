namespace Microsoft.AspNetCore.Routing;

using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Automations;
using TurtlePath.Automations.AspNetCore;
using TurtlePath.Automations.Descriptors;
using TurtlePath.Spider;

/// <summary>
/// Maps ASP.NET Core endpoints declared by TurtlePath automation metadata.
/// </summary>
public static class AutomationEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Discovers automation descriptors from the supplied assemblies and maps their endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapTurtlePathAutomationEndpoints(
        this IEndpointRouteBuilder endpoints,
        Action<AutomationEndpointRouteOptions> configure = null,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapTurtlePathAutomationEndpoints(
            AutomationDescriptorDiscovery.Discover(assemblies),
            configure);
    }

    /// <summary>
    /// Maps ASP.NET Core endpoints from prebuilt automation descriptors.
    /// </summary>
    public static IEndpointRouteBuilder MapTurtlePathAutomationEndpoints(
        this IEndpointRouteBuilder endpoints,
        IEnumerable<AutomationDescriptor> descriptors,
        Action<AutomationEndpointRouteOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(descriptors);

        var options = new AutomationEndpointRouteOptions();
        configure?.Invoke(options);

        foreach (var descriptor in descriptors)
        {
            if (!TryGetOptions(descriptor, out var endpointOptions))
                continue;

            var route = CombineRoute(options.RoutePrefix, endpointOptions.Route);
            var method = endpointOptions.Method ?? InferMethod(descriptor.OperationKind);
            var builder = endpoints.MapMethods(
                route,
                [ method ],
                context => InvokeAsync(context, descriptor, endpointOptions));

            builder.WithDisplayName(endpointOptions.Name ?? CreateEndpointName(descriptor));
            builder.WithTags(GetEndpointTag(endpointOptions.Route, descriptor.EntityType));
            builder.WithMetadata(typeof(AutomationEndpointRouteBuilderExtensions)
                .GetMethod(nameof(OpenApiEndpoint), BindingFlags.NonPublic | BindingFlags.Static));

            var groupName = GetOpenApiGroupName(options.RoutePrefix);
            if (!string.IsNullOrWhiteSpace(groupName))
                builder.WithGroupName(groupName);

            if (RequiresBody(descriptor.OperationKind))
                builder.WithMetadata(new AcceptsMetadata([ "application/json" ], descriptor.RequestType, false));

            if (descriptor.HasResponse)
                builder.WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status200OK, descriptor.ResponseType, [ "application/json" ]));
            else
                builder.WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status204NoContent, null, []));

            if (!string.IsNullOrWhiteSpace(endpointOptions.Name))
                builder.WithName(endpointOptions.Name);

            foreach (var item in GetEndpointMetadata(descriptor, endpointOptions))
                builder.WithMetadata(item);
        }

        return endpoints;
    }

    private static bool TryGetOptions(AutomationDescriptor descriptor, out AutomationEndpointOptions options)
    {
        options = null;

        if (descriptor?.Metadata == null ||
            !descriptor.Metadata.TryGetValue(AutomationEndpointMetadata.Endpoint, out var value))
        {
            return false;
        }

        options = value as AutomationEndpointOptions ??
            throw new InvalidOperationException($"Automation metadata '{AutomationEndpointMetadata.Endpoint}' must be an {nameof(AutomationEndpointOptions)} instance.");

        return true;
    }

    private static IEnumerable<object> GetEndpointMetadata(
        AutomationDescriptor descriptor,
        AutomationEndpointOptions endpointOptions)
    {
        if (descriptor.Metadata.TryGetValue(AutomationEndpointMetadata.EndpointDefaults, out var defaultsValue))
        {
            var defaults = defaultsValue as AutomationEndpointDefaults ??
                throw new InvalidOperationException($"Automation metadata '{AutomationEndpointMetadata.EndpointDefaults}' must be an {nameof(AutomationEndpointDefaults)} instance.");

            foreach (var item in defaults.Metadata)
                yield return item;
        }

        foreach (var item in endpointOptions.Metadata)
            yield return item;
    }

    private static async Task InvokeAsync(
        HttpContext context,
        AutomationDescriptor descriptor,
        AutomationEndpointOptions endpointOptions)
    {
        var request = await CreateRequestAsync(context, descriptor, endpointOptions);
        var cancellationToken = context.RequestAborted;

        if (descriptor.HasResponse)
        {
            var method = typeof(AutomationEndpointRouteBuilderExtensions)
                .GetMethod(nameof(DispatchResponseRequestAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(descriptor.RequestType, descriptor.ResponseType);
            var response = await (Task<object>)method.Invoke(null, [ context.RequestServices, request, cancellationToken ]);

            await Results.Json(response, ResolveJsonOptions(context)).ExecuteAsync(context);
            return;
        }

        var noResponseMethod = typeof(AutomationEndpointRouteBuilderExtensions)
            .GetMethod(nameof(DispatchNoResponseRequestAsync), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(descriptor.RequestType);
        await (Task)noResponseMethod.Invoke(null, [ context.RequestServices, request, cancellationToken ]);

        context.Response.StatusCode = StatusCodes.Status204NoContent;
    }

    private static async Task<object> CreateRequestAsync(
        HttpContext context,
        AutomationDescriptor descriptor,
        AutomationEndpointOptions endpointOptions)
    {
        var bindingContext = new AutomationEndpointBindingContext(context);
        var bindingValues = endpointOptions.BindingFactory?.Invoke(bindingContext);

        var request = RequiresBody(descriptor.OperationKind)
            ? await context.Request.ReadFromJsonAsync(
                descriptor.RequestType,
                ResolveJsonOptions(context),
                context.RequestAborted)
            : CreateQueryRequest(context, descriptor.RequestType, descriptor.KeyType, bindingValues);

        if (request is null)
            throw new BadHttpRequestException($"Request body could not be read as {descriptor.RequestType.Name}.");

        ApplyRouteId(context, request, descriptor.KeyType);
        ApplyBindingValues(request, bindingValues);
        ApplyQueryValues(context, request);
        endpointOptions.RequestBinder?.Invoke(request, bindingContext);

        return request;
    }

    private static Task OpenApiEndpoint(HttpContext context)
        => Task.CompletedTask;

    private static System.Text.Json.JsonSerializerOptions ResolveJsonOptions(HttpContext context)
        => context.RequestServices.GetService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()?.Value.JsonSerializerOptions ??
            context.RequestServices.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()?.Value.SerializerOptions;

    private static bool RequiresBody(AutomationOperationKind operationKind)
        => operationKind is AutomationOperationKind.Create or
            AutomationOperationKind.Update or
            AutomationOperationKind.Patch or
            AutomationOperationKind.Delete;

    private static object CreateQueryRequest(HttpContext context, Type requestType, Type keyType, object bindingValues)
    {
        if (TryCreateFromBindingValues(requestType, bindingValues, out var boundRequest))
            return boundRequest;

        if (context.Request.RouteValues.TryGetValue("id", out var idValue))
        {
            var convertedId = ConvertValue(idValue?.ToString(), keyType);
            var constructor = requestType.GetConstructors()
                .FirstOrDefault(item =>
                {
                    var parameters = item.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == keyType;
                });

            if (constructor is not null)
                return constructor.Invoke([ convertedId ]);
        }

        var pagedConstructor = requestType.GetConstructors()
            .FirstOrDefault(item =>
            {
                var parameters = item.GetParameters();
                return parameters.Length == 1 && string.Equals(parameters[0].ParameterType.Name, "PagedSettings", StringComparison.Ordinal);
            });

        if (pagedConstructor is not null)
        {
            var settingsType = pagedConstructor.GetParameters()[0].ParameterType;
            return pagedConstructor.Invoke([ CreatePagedSettings(context, settingsType) ]);
        }

        return Activator.CreateInstance(requestType) ??
            throw new InvalidOperationException($"Request type '{requestType.FullName}' must expose a public parameterless constructor or a constructor that receives the route id.");
    }

    private static bool TryCreateFromBindingValues(Type requestType, object bindingValues, out object request)
    {
        request = null;

        var values = ReadBindingValues(bindingValues);
        if (values.Count == 0)
            return false;

        foreach (var constructor in requestType.GetConstructors().OrderByDescending(item => item.GetParameters().Length))
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
                continue;

            var arguments = new object[parameters.Length];
            var matched = true;

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];

                if (!values.TryGetValue(parameter.Name ?? string.Empty, out var value) &&
                    !(parameters.Length == 1 && values.Count == 1 && values.TryGetValue(values.First().Key, out value)))
                {
                    matched = false;
                    break;
                }

                arguments[index] = ConvertValue(value, parameter.ParameterType);
            }

            if (!matched)
                continue;

            request = constructor.Invoke(arguments);
            return true;
        }

        return false;
    }

    private static void ApplyRouteId(HttpContext context, object request, Type keyType)
    {
        if (!context.Request.RouteValues.TryGetValue("id", out var idValue))
            return;

        var property = request.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
        if (property is null || !property.CanWrite)
            return;

        property.SetValue(request, ConvertValue(idValue?.ToString(), property.PropertyType == typeof(object) ? keyType : property.PropertyType));
    }

    private static void ApplyBindingValues(object request, object bindingValues)
    {
        var values = ReadBindingValues(bindingValues);
        if (values.Count == 0)
            return;

        var properties = request.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(item => item.CanWrite)
            .ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var item in values)
        {
            if (!properties.TryGetValue(item.Key, out var property))
                continue;

            property.SetValue(request, ConvertValue(item.Value, property.PropertyType));
        }
    }

    private static void ApplyQueryValues(HttpContext context, object request)
    {
        foreach (var property in request.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanWrite ||
                string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase) ||
                !context.Request.Query.TryGetValue(property.Name, out var value))
            {
                continue;
            }

            property.SetValue(request, ConvertValue(value.ToString(), property.PropertyType));
        }
    }

    private static Dictionary<string, object> ReadBindingValues(object bindingValues)
    {
        if (bindingValues is null)
            return [];

        return bindingValues.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(item => item.CanRead)
            .ToDictionary(item => item.Name, item => item.GetValue(bindingValues), StringComparer.OrdinalIgnoreCase);
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value is null)
            return null;

        var nullableType = Nullable.GetUnderlyingType(targetType);
        var effectiveType = nullableType ?? targetType;

        if (effectiveType.IsInstanceOfType(value))
            return value;

        return ConvertValue(value.ToString(), targetType);
    }

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

    private static object CreatePagedSettings(HttpContext context, Type settingsType)
    {
        var settings = Activator.CreateInstance(settingsType) ??
            throw new InvalidOperationException($"Paged settings type '{settingsType.FullName}' must expose a public parameterless constructor.");

        ApplyQueryValues(context, settings);

        return settings;
    }

    private static async Task<object> DispatchResponseRequestAsync<TRequest, TResponse>(
        IServiceProvider services,
        object request,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
    {
        var typedRequest = (TRequest)request;
        var spider = services.GetService<ISpider>();

        if (spider is not null)
            return await spider.DefaultSend<TRequest, TResponse>(typedRequest, cancellationToken);

        return await services.GetRequiredService<IMediator>().Send(typedRequest, cancellationToken);
    }

    private static async Task DispatchNoResponseRequestAsync<TRequest>(
        IServiceProvider services,
        object request,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest
    {
        var typedRequest = (TRequest)request;
        var spider = services.GetService<ISpider>();

        if (spider is not null)
        {
            await spider.DefaultSend(typedRequest, cancellationToken);
            return;
        }

        await services.GetRequiredService<IMediator>().Send(typedRequest, cancellationToken);
    }

    private static string InferMethod(AutomationOperationKind operationKind)
        => operationKind switch
        {
            AutomationOperationKind.Create => HttpMethods.Post,
            AutomationOperationKind.Update => HttpMethods.Put,
            AutomationOperationKind.Patch => HttpMethods.Patch,
            AutomationOperationKind.Delete => HttpMethods.Delete,
            _ => HttpMethods.Get
        };

    private static string CombineRoute(string prefix, string route)
        => string.Join(
            '/',
            new[] { prefix, route }
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim('/')));

    private static string CreateEndpointName(AutomationDescriptor descriptor)
        => $"{descriptor.OperationKind}{GetFriendlyName(descriptor.EntityType)}";

    private static string GetEndpointTag(string route, Type entityType)
    {
        var segment = route?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(item => !item.StartsWith('{'));

        return string.IsNullOrWhiteSpace(segment)
            ? GetFriendlyName(entityType)
            : char.ToUpperInvariant(segment[0]) + segment[1..];
    }

    private static string GetOpenApiGroupName(string routePrefix)
        => routePrefix?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(item => item.Length > 1 && item[0] == 'v' && char.IsDigit(item[1]));

    private static string GetFriendlyName(Type type)
        => type.IsGenericType ? type.Name[..type.Name.IndexOf('`')] : type.Name;
}
