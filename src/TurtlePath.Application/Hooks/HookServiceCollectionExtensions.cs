namespace TurtlePath.Application.Hooks
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Reflection;

    /// <summary>
    /// Provides dependency injection helpers for handler hooks.
    /// </summary>
    public static class HookServiceCollectionExtensions
    {
        private static readonly HashSet<Type> HookDefinitions = new()
        {
            typeof(IBeforeValidationHook<,>),
            typeof(IAfterValidationHook<,>),
            typeof(IBeforeGetEntityHook<,>),
            typeof(IAfterGetEntityHook<,>),
            typeof(IBeforeMapHook<,>),
            typeof(IAfterMapHook<,>),
            typeof(IBeforePatchHook<,>),
            typeof(IAfterPatchHook<,>),
            typeof(IBeforeSaveHook<,>),
            typeof(IAfterSaveHook<,>),
            typeof(IBeforeDeleteHook<,>),
            typeof(IAfterDeleteHook<,>),
            typeof(IBeforeResponseHook<,,>),
            typeof(IAfterResponseHook<,,>),
            typeof(IBeforeQueryHook<,>),
            typeof(IAfterQueryHook<,>)
        };

        /// <summary>
        /// Registers all hook interfaces implemented by the specified hook type.
        /// </summary>
        /// <typeparam name="THook">The hook implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddHandlerHook<THook>(this IServiceCollection services)
            where THook : class
            => services.AddHandlerHook(typeof(THook));

        /// <summary>
        /// Registers all hook interfaces implemented by the specified hook type.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hookType">The hook implementation type.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddHandlerHook(this IServiceCollection services, Type hookType)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (hookType == null)
                throw new ArgumentNullException(nameof(hookType));

            if (hookType.IsAbstract || hookType.IsInterface)
                throw new ArgumentException("Hook type must be a concrete class.", nameof(hookType));

            foreach (var serviceType in GetHookServiceTypes(hookType))
                services.TryAddEnumerable(ServiceDescriptor.Scoped(serviceType, hookType));

            return services;
        }

        /// <summary>
        /// Registers hook implementations discovered in the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddHandlerHooksFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            foreach (var assembly in assemblies.Where(a => a != null).Distinct())
            {
                foreach (var hookType in assembly.GetTypes().Where(IsConcreteHookType))
                    services.AddHandlerHook(hookType);
            }

            return services;
        }

        /// <summary>
        /// Registers hook implementations discovered in the assembly that contains the specified marker type.
        /// </summary>
        /// <typeparam name="TMarker">A type from the assembly to scan.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddHandlerHooksFromAssemblyContaining<TMarker>(this IServiceCollection services)
            => services.AddHandlerHooksFromAssemblies(typeof(TMarker).Assembly);

        private static bool IsConcreteHookType(Type type)
            => type.IsClass && !type.IsAbstract && GetHookServiceTypes(type).Any();

        private static IEnumerable<Type> GetHookServiceTypes(Type hookType)
        {
            foreach (var interfaceType in hookType.GetInterfaces())
            {
                if (!interfaceType.IsGenericType)
                    continue;

                var hookDefinition = interfaceType.GetGenericTypeDefinition();

                if (!HookDefinitions.Contains(hookDefinition))
                    continue;

                yield return interfaceType.ContainsGenericParameters
                    ? hookDefinition
                    : interfaceType;
            }
        }
    }
}
