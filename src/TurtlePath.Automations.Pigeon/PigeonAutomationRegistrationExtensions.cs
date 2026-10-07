namespace Microsoft.Extensions.DependencyInjection;

using System.Reflection;
using Pelican.Mediator;
using Pigeon.Messaging.Consuming.Dispatching;
using Pigeon.Messaging.Contracts;
using Spider.Pipelines.Core;
using TurtlePath.Automations;
using TurtlePath.Automations.Descriptors;
using TurtlePath.Automations.Pigeon;
using TurtlePath.Spider;

/// <summary>
/// Registers Pigeon consumers declared by TurtlePath automation metadata.
/// </summary>
public static class PigeonAutomationRegistrationExtensions
{
    /// <summary>
    /// Discovers automation descriptors from the supplied assemblies and registers their Pigeon consumers.
    /// </summary>
    /// <param name="builder">The Pigeon service builder.</param>
    /// <param name="assemblies">Assemblies that contain automation profiles or attributes.</param>
    /// <returns>The same Pigeon service builder.</returns>
    public static IPigeonServiceBuilder AddAutomationConsumers(
        this IPigeonServiceBuilder builder,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddAutomationConsumers(AutomationDescriptorDiscovery.Discover(assemblies));
    }

    /// <summary>
    /// Registers Pigeon consumers from prebuilt automation descriptors.
    /// </summary>
    /// <param name="builder">The Pigeon service builder.</param>
    /// <param name="descriptors">Automation descriptors to inspect.</param>
    /// <returns>The same Pigeon service builder.</returns>
    public static IPigeonServiceBuilder AddAutomationConsumers(
        this IPigeonServiceBuilder builder,
        IEnumerable<AutomationDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(descriptors);

        foreach (var descriptor in descriptors)
        {
            if (!TryGetOptions(descriptor, out var options))
                continue;

            RegisterConsumer(builder, descriptor, options);
        }

        return builder;
    }

    private static bool TryGetOptions(
        AutomationDescriptor descriptor,
        out AutomationPigeonConsumerOptions options)
    {
        options = null;

        if (descriptor?.Metadata == null ||
            !descriptor.Metadata.TryGetValue(AutomationPigeonMetadata.Consumer, out var value))
        {
            return false;
        }

        options = value as AutomationPigeonConsumerOptions ??
            throw new InvalidOperationException($"Automation metadata '{AutomationPigeonMetadata.Consumer}' must be an {nameof(AutomationPigeonConsumerOptions)} instance.");

        return true;
    }

    private static void RegisterConsumer(
        IPigeonServiceBuilder builder,
        AutomationDescriptor descriptor,
        AutomationPigeonConsumerOptions options)
    {
        var version = SemanticVersion.Parse(options.Version);

        if (descriptor.HasResponse)
        {
            var method = typeof(PigeonAutomationRegistrationExtensions)
                .GetMethod(nameof(RegisterResponseConsumer), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(descriptor.RequestType, descriptor.ResponseType);
            method.Invoke(null, [ builder, options.Topic, version, options.Subscription ]);
            return;
        }

        var noResponseMethod = typeof(PigeonAutomationRegistrationExtensions)
            .GetMethod(nameof(RegisterNoResponseConsumer), BindingFlags.NonPublic | BindingFlags.Static)
            .MakeGenericMethod(descriptor.RequestType);
        noResponseMethod.Invoke(null, [ builder, options.Topic, version, options.Subscription ]);
    }

    private static void RegisterResponseConsumer<TRequest, TResponse>(
        IPigeonServiceBuilder builder,
        string topic,
        SemanticVersion version,
        string subscription)
        where TRequest : class, IRequest<TResponse>
    {
        builder.AddConsumeHandler<TRequest>(
            topic,
            version,
            subscription,
            HandleResponseRequest<TRequest, TResponse>);
    }

    private static void RegisterNoResponseConsumer<TRequest>(
        IPigeonServiceBuilder builder,
        string topic,
        SemanticVersion version,
        string subscription)
        where TRequest : class, IRequest
    {
        builder.AddConsumeHandler<TRequest>(
            topic,
            version,
            subscription,
            HandleNoResponseRequest<TRequest>);
    }

    private static async Task HandleResponseRequest<TRequest, TResponse>(
        ConsumeContext context,
        TRequest request)
        where TRequest : class, IRequest<TResponse>
    {
        var spider = context.Services.GetService<ISpider>();
        if (spider is not null)
        {
            await spider.DefaultSend<TRequest, TResponse>(request, context.CancellationToken);
            return;
        }

        await context.Services.GetRequiredService<IMediator>().Send(request, context.CancellationToken);
    }

    private static async Task HandleNoResponseRequest<TRequest>(
        ConsumeContext context,
        TRequest request)
        where TRequest : class, IRequest
    {
        var spider = context.Services.GetService<ISpider>();
        if (spider is not null)
        {
            await spider.DefaultSend(request, context.CancellationToken);
            return;
        }

        await context.Services.GetRequiredService<IMediator>().Send(request, context.CancellationToken);
    }
}
