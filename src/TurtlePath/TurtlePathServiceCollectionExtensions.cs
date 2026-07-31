namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System.Reflection;
    using TurtlePath;
    using TurtlePath.Commands.Steps;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Hooks;

    /// <summary>
    /// Provides registration helpers for the main TurtlePath package.
    /// </summary>
    public static class TurtlePathServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath hook discovery for the supplied assemblies.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="hookAssemblies">Assemblies that contain TurtlePath hooks.</param>
        /// <returns>The same service collection.</returns>
        public static ITurtlePathBuilder AddTurtlePath(this IServiceCollection services, params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddScoped<IHandlerHookRunner, HandlerHookRunner>();
            services.TryAddScoped(typeof(ICommandHookStageRunner<,>), typeof(CommandHookStageRunner<,>));
            services.TryAddScoped(typeof(ICommandHookStageRunner<,,>), typeof(CommandHookStageRunner<,,>));
            services.TryAddScoped(typeof(IQueryHookStageRunner<,>), typeof(QueryHookStageRunner<,>));
            services.TryAddScoped(typeof(IRequestValidationStep<,>), typeof(DefaultRequestValidationStep<,>));
            services.TryAddScoped(typeof(IEntityCreationStep<,>), typeof(DefaultEntityCreationStep<,>));
            services.TryAddScoped(typeof(IEntityLookupStep<,,>), typeof(DefaultEntityLookupStep<,,>));
            services.TryAddScoped(typeof(IEntityMappingStep<,>), typeof(DefaultEntityMappingStep<,>));
            services.TryAddScoped(typeof(IEntityAddStep<,>), typeof(DefaultEntityAddStep<,>));
            services.TryAddScoped(typeof(IEntitySaveStep<,>), typeof(DefaultEntitySaveStep<,>));
            services.TryAddScoped(typeof(IEntityDeleteStep<,>), typeof(DefaultEntityDeleteStep<,>));
            services.TryAddScoped(typeof(IResponseMappingStep<,,,>), typeof(DefaultResponseMappingStep<,,,>));

            if (hookAssemblies?.Length > 0)
                services.AddHandlerHooksFromAssemblies(hookAssemblies);

            return new TurtlePathBuilder(services);
        }

        /// <summary>
        /// Registers TurtlePath identifier configuration and hook discovery for the supplied assemblies.
        /// </summary>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <param name="hookAssemblies">Assemblies that contain TurtlePath hooks.</param>
        /// <returns>The same service collection.</returns>
        public static ITurtlePathBuilder AddTurtlePath<TTargetType, TDbType>(
            this IServiceCollection services,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier,
            params Assembly[] hookAssemblies)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builder = services.AddTurtlePath(hookAssemblies);
            builder.UseCId(configureIdentifier);

            return builder;
        }

        /// <summary>
        /// Configures TurtlePath identifiers on the current registration pipeline.
        /// </summary>
        /// <typeparam name="TTargetType">The domain identifier value type.</typeparam>
        /// <typeparam name="TDbType">The database identifier value type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureIdentifier">The CId configuration callback.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCId<TTargetType, TDbType>(
            this ITurtlePathBuilder builder,
            Action<CIdConfiguration<TTargetType, TDbType>> configureIdentifier)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var profileBuilder = new CIdProfileBuilder(GetOrCreateRegistry(builder.Services));
            profileBuilder.UseCId(configureIdentifier);

            return builder;
        }

        /// <summary>
        /// Configures TurtlePath identifiers from a profile.
        /// </summary>
        /// <typeparam name="TProfile">The CId profile type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCIdProfile<TProfile>(this ITurtlePathBuilder builder)
            where TProfile : ICIdProfile, new()
        {
            return builder.UseCIdProfile(new TProfile());
        }

        /// <summary>
        /// Configures TurtlePath identifiers from a profile instance.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="profile">The CId profile.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCIdProfile(this ITurtlePathBuilder builder, ICIdProfile profile)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            profile.Configure(new CIdProfileBuilder(GetOrCreateRegistry(builder.Services)));

            return builder;
        }

        /// <summary>
        /// Discovers and configures TurtlePath identifier profiles from the supplied assemblies.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="profileAssemblies">Assemblies that contain CId profiles.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCIdProfiles(this ITurtlePathBuilder builder, params Assembly[] profileAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
                return builder;

            var profiles = profileAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(ICIdProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (ICIdProfile)Activator.CreateInstance(type, nonPublic: true));

            foreach (var profile in profiles)
                builder.UseCIdProfile(profile);

            return builder;
        }

        private static CIdDefinitionRegistry GetOrCreateRegistry(IServiceCollection services)
        {
            var registry = services
                .Select(descriptor => descriptor.ImplementationInstance)
                .OfType<CIdDefinitionRegistry>()
                .FirstOrDefault();

            if (registry != null)
                return registry;

            registry = new CIdDefinitionRegistry();
            services.AddSingleton(registry);
            services.AddSingleton<ICIdDefinitionRegistry>(registry);
            services.AddSingleton<ICIdFactory>(registry);

            return registry;
        }
    }
}
