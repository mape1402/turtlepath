namespace TurtlePath.EventSourcing
{
    using Krackend.EventSourcing.Configuration;
    using Krackend.EventSourcing.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;
    using TurtlePath.EventSourcing.Internal;

    /// <summary>
    /// Provides EventSourcing registration helpers for TurtlePath.
    /// </summary>
    public static class EventSourcingTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Enables Krackend event sourcing for the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configure">Optional Krackend event sourcing configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEventSourcing(
            this ITurtlePathBuilder builder,
            Action<EventSourcingOptions> configure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            EnsureEventSourcing(builder.Services, configure);

            return builder;
        }

        /// <summary>
        /// Enables Krackend event sourcing and adds mappings through a compact profile builder.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configureMappings">The mapping configuration.</param>
        /// <param name="configure">Optional Krackend event sourcing configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEventSourcing(
            this ITurtlePathBuilder builder,
            Action<IEventSourcingProfileBuilder> configureMappings,
            Action<EventSourcingOptions> configure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configureMappings == null)
                throw new ArgumentNullException(nameof(configureMappings));

            var profileBuilder = new EventSourcingProfileBuilder(builder.Services, GetOrCreateRegistry(builder.Services));
            configureMappings(profileBuilder);
            EnsureEventSourcing(builder.Services, ComposeConfiguration(configure, profileBuilder.EventAssemblies));

            return builder;
        }

        /// <summary>
        /// Adds event mappings from a profile.
        /// </summary>
        /// <typeparam name="TProfile">The event sourcing profile type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configure">Optional Krackend event sourcing configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEventSourcingProfile<TProfile>(
            this ITurtlePathBuilder builder,
            Action<EventSourcingOptions> configure = null)
            where TProfile : IEventSourcingProfile, new()
        {
            return builder.UseEventSourcingProfile(new TProfile(), configure);
        }

        /// <summary>
        /// Adds event mappings from a profile instance.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="profile">The event sourcing profile.</param>
        /// <param name="configure">Optional Krackend event sourcing configuration.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEventSourcingProfile(
            this ITurtlePathBuilder builder,
            IEventSourcingProfile profile,
            Action<EventSourcingOptions> configure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var profileBuilder = new EventSourcingProfileBuilder(builder.Services, GetOrCreateRegistry(builder.Services));
            profile.Configure(profileBuilder);
            EnsureEventSourcing(builder.Services, ComposeConfiguration(configure, profileBuilder.EventAssemblies));

            return builder;
        }

        /// <summary>
        /// Discovers and adds event sourcing profiles from the supplied assemblies.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="profileAssemblies">Assemblies that contain event sourcing profiles.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEventSourcingProfiles(
            this ITurtlePathBuilder builder,
            params Assembly[] profileAssemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (profileAssemblies == null || profileAssemblies.Length == 0)
            {
                EnsureEventSourcing(builder.Services);
                return builder;
            }

            var profiles = profileAssemblies
                .Where(assembly => assembly != null)
                .Distinct()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IEventSourcingProfile).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    type.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null) != null)
                .Select(type => (IEventSourcingProfile)Activator.CreateInstance(type, nonPublic: true));

            var profileBuilder = new EventSourcingProfileBuilder(builder.Services, GetOrCreateRegistry(builder.Services));

            foreach (var profile in profiles)
                profile.Configure(profileBuilder);

            EnsureEventSourcing(builder.Services, ComposeConfiguration(null, profileBuilder.EventAssemblies));

            return builder;
        }

        private static Action<EventSourcingOptions> ComposeConfiguration(
            Action<EventSourcingOptions> configure,
            IEnumerable<Assembly> eventAssemblies)
        {
            return options =>
            {
                if (eventAssemblies != null)
                {
                    foreach (var assembly in eventAssemblies.Where(assembly => assembly != null).Distinct())
                        options.ScanAssembly(assembly);
                }

                configure?.Invoke(options);
            };
        }

        private static void EnsureEventSourcing(
            IServiceCollection services,
            Action<EventSourcingOptions> configure = null)
        {
            if (services.Any(descriptor => descriptor.ServiceType == typeof(EventSourcingRegistrationMarker)))
                return;

            services.AddKrackendEventSourcing(configure);
            GetOrCreateRegistry(services);
            services.AddSingleton<EventSourcingRegistrationMarker>();
        }

        private static EventSourcingRegistrationRegistry GetOrCreateRegistry(IServiceCollection services)
        {
            var registry = services
                .Select(descriptor => descriptor.ImplementationInstance)
                .OfType<EventSourcingRegistrationRegistry>()
                .FirstOrDefault();

            if (registry != null)
                return registry;

            registry = new EventSourcingRegistrationRegistry();
            services.AddSingleton(registry);

            return registry;
        }

        private sealed class EventSourcingRegistrationMarker
        {
        }
    }
}
