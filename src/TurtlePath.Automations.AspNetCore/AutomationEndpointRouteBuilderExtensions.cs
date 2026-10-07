namespace Microsoft.AspNetCore.Routing;

using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
                context => InvokeAsync(context, descriptor));

            builder.WithDisplayName(endpointOptions.Name ?? CreateEndpointName(descriptor));
            builder.WithTags(GetFriendlyName(descriptor.EntityType));

            if (!string.IsNullOrWhiteSpace(endpointOptions.Name))
                builder.WithName(endpointOptions.Name);
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

    private static async Task InvokeAsync(HttpContext context, AutomationDescriptor descriptor)
    {
        var request = await CreateRequestAsync(context, descriptor);
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

    private static async Task<object> CreateRequestAsync(HttpContext context, AutomationDescriptor descriptor)
    {
        var request = RequiresBody(descriptor.OperationKind)
            ? await context.Request.ReadFromJsonAsync(
                descriptor.RequestType,
                ResolveJsonOptions(context),
                context.RequestAborted)
            : CreateQueryRequest(context, descriptor.RequestType, descriptor.KeyType);

        if (request is null)
            throw new BadHttpRequestException($"Request body could not be read as {descriptor.RequestType.Name}.");

        ApplyRouteId(context, request, descriptor.KeyType);
        ApplyQueryValues(context, request);

        return request;
    }

    private static System.Text.Json.JsonSerializerOptions ResolveJsonOptions(HttpContext context)
        => context.RequestServices.GetService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()?.Value.JsonSerializerOptions ??
            context.RequestServices.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()?.Value.SerializerOptions;

    private static bool RequiresBody(AutomationOperationKind operationKind)
        => operationKind is AutomationOperationKind.Create or
            AutomationOperationKind.Update or
            AutomationOperationKind.Patch or
            AutomationOperationKind.Delete;

    private static object CreateQueryRequest(HttpContext context, Type requestType, Type keyType)
    {
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

        return Activator.CreateInstance(requestType) ??
            throw new InvalidOperationException($"Request type '{requestType.FullName}' must expose a public parameterless constructor or a constructor that receives the route id.");
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

    private static string GetFriendlyName(Type type)
        => type.IsGenericType ? type.Name[..type.Name.IndexOf('`')] : type.Name;
}
