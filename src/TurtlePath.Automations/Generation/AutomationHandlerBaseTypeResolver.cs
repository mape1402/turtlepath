namespace TurtlePath.Automations.Generation
{
    using TurtlePath.Automations.Descriptors;
    using TurtlePath.Models.Responses;

    internal sealed class AutomationHandlerBaseTypeResolver : IAutomationHandlerBaseTypeResolver
    {
        public Type Resolve(AutomationDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

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
