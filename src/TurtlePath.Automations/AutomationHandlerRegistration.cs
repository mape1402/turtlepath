namespace TurtlePath.Automations
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Generation;
    using TurtlePath.Automations.Options;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    internal static class AutomationHandlerRegistration
    {
        public static void Register(IServiceCollection services, IEnumerable<AutomationDescriptor> descriptors)
        {
            var registry = new AutomationDescriptorRegistry(descriptors);
            services.TryAddSingleton(registry);

            RegisterQueryOptions(services, registry.Descriptors);

            var generatedTypes = new AutomationHandlerTypeGenerator().Generate(registry.Descriptors);
            foreach (var descriptor in registry.Descriptors)
                Register(services, descriptor, generatedTypes[descriptor]);
        }

        private static void Register(IServiceCollection services, AutomationDescriptor descriptor, Type implementationType)
        {
            var serviceType = descriptor.HasResponse
                ? typeof(IRequestHandler<,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType)
                : typeof(IRequestHandler<>).MakeGenericType(descriptor.RequestType);

            services.AddScoped(serviceType, implementationType);
        }

        internal static Type ResolveBaseHandlerType(AutomationDescriptor descriptor)
        {
            try
            {
                return descriptor.OperationKind switch
                {
                    AutomationOperationKind.Create when descriptor.HasResponse => typeof(TurtlePath.Commands.GenericCreateCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Create => typeof(TurtlePath.Commands.GenericCreateCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Update when descriptor.HasResponse => typeof(TurtlePath.Commands.GenericUpdateCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Update => typeof(TurtlePath.Commands.GenericUpdateCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Delete when descriptor.HasResponse => typeof(TurtlePath.Commands.GenericDeleteCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Delete => typeof(TurtlePath.Commands.GenericDeleteCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Patch when descriptor.HasResponse => typeof(TurtlePath.Commands.GenericPatchCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Patch => typeof(TurtlePath.Commands.GenericPatchCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.GetById => typeof(TurtlePath.Queries.GenericGetByIdQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.ResponseType, descriptor.KeyType),
                    AutomationOperationKind.GetOne => typeof(TurtlePath.Queries.GenericGetOneQueryHandler<,,,,>).MakeGenericType(descriptor.RequestType, ResolveGetOneValueType(descriptor), descriptor.EntityType, descriptor.ResponseType, descriptor.KeyType),
                    AutomationOperationKind.GetMany => typeof(TurtlePath.Queries.GenericGetManyQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, ResolveCollectionItemType(descriptor.ResponseType), descriptor.KeyType),
                    AutomationOperationKind.GetPaged => typeof(TurtlePath.Queries.GenericGetPagedInfoQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, ResolvePagedItemType(descriptor.ResponseType), descriptor.KeyType),
                    _ => throw new NotSupportedException($"Automation operation '{descriptor.OperationKind}' is not supported by handler registration yet.")
                };
            }
            catch (ArgumentException exception)
            {
                throw new NotSupportedException(
                    $"Automation operation '{descriptor.OperationKind}' for request '{descriptor.RequestType.FullName}' cannot be registered with the current TurtlePath generic handler contracts.",
                    exception);
            }
        }

        private static void RegisterQueryOptions(IServiceCollection services, IEnumerable<AutomationDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.OperationKind == AutomationOperationKind.GetOne)
                {
                    var serviceType = typeof(IGetOneQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
                    var implementationType = typeof(DescriptorGetOneQueryOptions<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType, ResolveGetOneValueType(descriptor));
                    services.TryAddScoped(serviceType, implementationType);
                }

                if (descriptor.OperationKind == AutomationOperationKind.GetPaged)
                {
                    var serviceType = typeof(IGetPagedInfoQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
                    var implementationType = typeof(DescriptorGetPagedInfoQueryOptions<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, ResolvePagedItemType(descriptor.ResponseType));
                    services.TryAddScoped(serviceType, implementationType);
                }
            }
        }

        private static Type ResolveCollectionItemType(Type responseType)
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return responseType.GetGenericArguments()[0];

            return responseType;
        }

        private static Type ResolveGetOneValueType(AutomationDescriptor descriptor)
        {
            var queryBase = FindGenericBaseType(descriptor.RequestType, typeof(TurtlePath.Queries.GenericGetOneQuery<,,,>));
            if (queryBase != null)
                return queryBase.GetGenericArguments()[0];

            return descriptor.KeySelector?.ReturnType ?? descriptor.KeyType;
        }

        private static Type FindGenericBaseType(Type type, Type genericTypeDefinition)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericTypeDefinition)
                    return current;
            }

            return null;
        }

        private static Type ResolvePagedItemType(Type responseType)
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(PagedResponse<>))
                return responseType.GetGenericArguments()[0];

            return responseType;
        }
    }
}
