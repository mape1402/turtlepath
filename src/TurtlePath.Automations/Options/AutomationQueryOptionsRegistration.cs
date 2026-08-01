namespace TurtlePath.Automations.Options
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    internal sealed class AutomationQueryOptionsRegistration : IAutomationQueryOptionsRegistration
    {
        public void Register(IServiceCollection services, IReadOnlyCollection<AutomationDescriptor> descriptors)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (descriptors == null)
                throw new ArgumentNullException(nameof(descriptors));

            foreach (var descriptor in descriptors)
                Register(services, descriptor);
        }

        private void Register(IServiceCollection services, AutomationDescriptor descriptor)
        {
            if (descriptor.OperationKind == AutomationOperationKind.GetOne)
            {
                var serviceType = typeof(IGetOneQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
                var implementationType = typeof(DescriptorGetOneQueryOptions<,,,>).MakeGenericType(
                    descriptor.RequestType,
                    descriptor.EntityType,
                    descriptor.KeyType,
                    ResolveGetOneValueType(descriptor));

                services.TryAddScoped(serviceType, implementationType);
            }

            if (descriptor.OperationKind == AutomationOperationKind.GetPaged)
            {
                var serviceType = typeof(IGetPagedInfoQueryOptions<,>).MakeGenericType(descriptor.RequestType, descriptor.EntityType);
                var implementationType = typeof(DescriptorGetPagedInfoQueryOptions<,,>).MakeGenericType(
                    descriptor.RequestType,
                    descriptor.EntityType,
                    ResolvePagedItemType(descriptor.ResponseType));

                services.TryAddScoped(serviceType, implementationType);
            }
        }

        private Type ResolveGetOneValueType(AutomationDescriptor descriptor)
        {
            var queryBase = FindGenericBaseType(descriptor.RequestType, typeof(GenericGetOneQuery<,,,>));
            if (queryBase != null)
                return queryBase.GetGenericArguments()[0];

            return descriptor.KeySelector?.ReturnType ?? descriptor.KeyType;
        }

        private Type FindGenericBaseType(Type type, Type genericTypeDefinition)
        {
            for (var current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericTypeDefinition)
                    return current;
            }

            return null;
        }

        private Type ResolvePagedItemType(Type responseType)
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(PagedResponse<>))
                return responseType.GetGenericArguments()[0];

            return responseType;
        }
    }
}
