namespace TurtlePath.Automations.Attributes
{
    using System.Reflection;
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;

    internal static class AutomationAttributeDescriptorProvider
    {
        public static IReadOnlyCollection<AutomationDescriptor> GetDescriptors(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                return Array.Empty<AutomationDescriptor>();

            var registry = new AutomationDescriptorRegistry();

            foreach (var requestType in assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsClass && !type.IsAbstract))
            {
                foreach (var attribute in requestType.GetCustomAttributes<AutomationAttribute>(false))
                    registry.Add(CreateDescriptor(requestType, attribute));
            }

            return registry.Descriptors;
        }

        private static AutomationDescriptor CreateDescriptor(Type requestType, AutomationAttribute attribute)
        {
            var keyType = ResolveKeyType(attribute.EntityType);
            var responseType = ResolveResponseType(attribute);
            var returnMode = responseType == null ? AutomationReturnMode.None : AutomationReturnMode.Response;

            return new AutomationDescriptor(
                attribute.OperationKind,
                requestType,
                attribute.EntityType,
                keyType,
                returnMode,
                responseType,
                AutomationSourceKind.Attribute);
        }

        private static Type ResolveResponseType(AutomationAttribute attribute)
        {
            if (attribute.ResponseType == null)
                return null;

            if (attribute.OperationKind == AutomationOperationKind.GetMany)
                return typeof(IEnumerable<>).MakeGenericType(attribute.ResponseType);

            if (attribute.OperationKind == AutomationOperationKind.GetPaged)
                return typeof(PagedResponse<>).MakeGenericType(attribute.ResponseType);

            return attribute.ResponseType;
        }

        private static Type ResolveKeyType(Type entityType)
        {
            var entityContract = entityType
                .GetInterfaces()
                .Concat([entityType])
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEntity<>))
                .Select(type => type.GetGenericArguments()[0])
                .Distinct()
                .ToArray();

            return entityContract.Length == 1
                ? entityContract[0]
                : throw new InvalidOperationException($"Entity type '{entityType.FullName}' must implement exactly one IEntity<TKey> contract.");
        }
    }
}
