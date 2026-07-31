namespace TurtlePath.Automations
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Automations.Handlers;
    using TurtlePath.Models.Responses;

    internal static class AutomationHandlerRegistrar
    {
        public static void Register(IServiceCollection services, IEnumerable<AutomationDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
                Register(services, descriptor);
        }

        private static void Register(IServiceCollection services, AutomationDescriptor descriptor)
        {
            var serviceType = descriptor.HasResponse
                ? typeof(IRequestHandler<,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType)
                : typeof(IRequestHandler<>).MakeGenericType(descriptor.RequestType);

            var implementationType = ResolveImplementationType(descriptor);
            services.AddScoped(serviceType, implementationType);
        }

        private static Type ResolveImplementationType(AutomationDescriptor descriptor)
        {
            try
            {
                return descriptor.OperationKind switch
                {
                    AutomationOperationKind.Create when descriptor.HasResponse => typeof(AutomatedCreateCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Create => typeof(AutomatedCreateCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Update when descriptor.HasResponse => typeof(AutomatedUpdateCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Update => typeof(AutomatedUpdateCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Delete when descriptor.HasResponse => typeof(AutomatedDeleteCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Delete => typeof(AutomatedDeleteCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Patch when descriptor.HasResponse => typeof(AutomatedPatchCommandHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.ResponseType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.Patch => typeof(AutomatedPatchCommandHandler<,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.KeyType),
                    AutomationOperationKind.GetById => typeof(AutomatedGetByIdQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, descriptor.ResponseType, descriptor.KeyType),
                    AutomationOperationKind.GetMany => typeof(AutomatedGetManyQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, ResolveCollectionItemType(descriptor.ResponseType), descriptor.KeyType),
                    AutomationOperationKind.GetPaged => typeof(AutomatedGetPagedQueryHandler<,,,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType, ResolvePagedItemType(descriptor.ResponseType), descriptor.KeyType),
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

        private static Type ResolveCollectionItemType(Type responseType)
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return responseType.GetGenericArguments()[0];

            return responseType;
        }

        private static Type ResolvePagedItemType(Type responseType)
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(PagedResponse<>))
                return responseType.GetGenericArguments()[0];

            return responseType;
        }
    }
}
