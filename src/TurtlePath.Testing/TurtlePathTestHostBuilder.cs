namespace TurtlePath.Testing
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Pelican.Mediator;
    using TurtlePath.Automations;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;
    using TurtlePath.Hooks;
    using TurtlePath.Testing.Hooks;
    using TurtlePath.Testing.Mapping;
    using TurtlePath.Testing.Persistence;
    using TurtlePath.Testing.Validation;
    using TurtlePath.Validation;

    /// <summary>
    /// Fluent builder for TurtlePath test hosts.
    /// </summary>
    public sealed partial class TurtlePathTestHostBuilder
    {
        private readonly DelegateMapperAdapter mapper = new();
        private readonly DelegateValidatorAdapter validator = new();
        private readonly List<Action<IServiceCollection>> serviceConfigurations = [];
        private readonly List<Assembly> pelicanAssemblies = [];
        private readonly List<Assembly> hookAssemblies = [];
        private bool registerTurtlePath = true;
        private bool registerInMemoryStorage = true;
        private bool traceHooks;

        /// <summary>
        /// Gets the service collection being configured.
        /// </summary>
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// Configures TurtlePath registration.
        /// </summary>
        public TurtlePathTestHostBuilder UseTurtlePath(params Assembly[] assemblies)
        {
            registerTurtlePath = true;
            hookAssemblies.AddRange(assemblies ?? []);

            return this;
        }

        /// <summary>
        /// Registers Pelican handlers from the supplied assemblies.
        /// </summary>
        public TurtlePathTestHostBuilder UsePelican(params Assembly[] assemblies)
        {
            pelicanAssemblies.AddRange(assemblies ?? []);
            return this;
        }

        /// <summary>
        /// Registers TurtlePath automations from the supplied assemblies.
        /// </summary>
        public TurtlePathTestHostBuilder UseAutomations(params Assembly[] assemblies)
        {
            serviceConfigurations.Add(services =>
            {
                var builder = services.AddTurtlePath(hookAssemblies.Distinct().ToArray());
                builder.UseAutomations(assemblies ?? []);
            });
            registerTurtlePath = false;

            return UsePelican(assemblies ?? []);
        }

        /// <summary>
        /// Uses the in-memory TurtlePath storage adapters.
        /// </summary>
        public TurtlePathTestHostBuilder UseInMemoryStorage()
        {
            registerInMemoryStorage = true;
            return this;
        }

        /// <summary>
        /// Disables the default in-memory storage registration.
        /// </summary>
        public TurtlePathTestHostBuilder WithoutInMemoryStorage()
        {
            registerInMemoryStorage = false;
            return this;
        }

        /// <summary>
        /// Adds custom service registrations.
        /// </summary>
        public TurtlePathTestHostBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            serviceConfigurations.Add(configure);
            return this;
        }

        /// <summary>
        /// Registers a singleton service instance.
        /// </summary>
        public TurtlePathTestHostBuilder WithSingleton<TService>(TService implementation)
            where TService : class
            => ConfigureServices(services => services.AddSingleton(implementation));

        /// <summary>
        /// Registers a transient service.
        /// </summary>
        public TurtlePathTestHostBuilder WithTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => ConfigureServices(services => services.AddTransient<TService, TImplementation>());

        /// <summary>
        /// Registers a scoped service.
        /// </summary>
        public TurtlePathTestHostBuilder WithScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => ConfigureServices(services => services.AddScoped<TService, TImplementation>());

        /// <summary>
        /// Registers a mapping delegate.
        /// </summary>
        public TurtlePathTestHostBuilder WithMap<TSource, TDestination>(Func<TSource, TDestination> map)
            where TSource : class
            where TDestination : class
        {
            mapper.WithMap(map);
            return this;
        }

        /// <summary>
        /// Registers an update mapping delegate.
        /// </summary>
        public TurtlePathTestHostBuilder WithUpdateMap<TSource, TDestination>(Action<TSource, TDestination> map)
            where TSource : class
            where TDestination : class
        {
            mapper.WithUpdateMap(map);
            return this;
        }

        /// <summary>
        /// Registers a valid request model.
        /// </summary>
        public TurtlePathTestHostBuilder WithValidRequest<TRequest>()
        {
            validator.WithValidModel<TRequest>();
            return this;
        }

        /// <summary>
        /// Registers a validation delegate.
        /// </summary>
        public TurtlePathTestHostBuilder WithValidator<TRequest>(Func<TRequest, CancellationToken, ValueTask> validate)
        {
            validator.WithValidator(validate);
            return this;
        }

        /// <summary>
        /// Registers generic testing hooks that record command, response, and query hook stages.
        /// </summary>
        public TurtlePathTestHostBuilder TraceHooks()
        {
            traceHooks = true;
            return this;
        }

        /// <summary>
        /// Seeds entities into the in-memory store during build.
        /// </summary>
        public TurtlePathTestHostBuilder WithSeed<TEntity>(params TEntity[] entities)
            where TEntity : class, TurtlePath.Domain.Contracts.IEntity
            => ConfigureServices(services => services.AddSingleton<ISeedRegistration>(new SeedRegistration<TEntity>(entities)));

        /// <summary>
        /// Builds the test host.
        /// </summary>
        public ValueTask<TurtlePathTestHost> BuildAsync()
        {
            Services.TryAddSingleton<IMapperAdapter>(mapper);
            Services.TryAddSingleton<IValidatorAdapter>(validator);

            if (registerTurtlePath)
                Services.AddTurtlePath(hookAssemblies.Distinct().ToArray());

            if (pelicanAssemblies.Count > 0)
                Services.AddPelican(pelicanAssemblies.Distinct().ToArray());

            if (registerInMemoryStorage)
            {
                Services.TryAddSingleton<InMemoryTurtlePathStorage>();
                Services.TryAddSingleton<IStorageReaderAdapter>(provider => provider.GetRequiredService<InMemoryTurtlePathStorage>());
                Services.TryAddSingleton<IStorageWriterAdapter>(provider => provider.GetRequiredService<InMemoryTurtlePathStorage>());
            }

            if (traceHooks)
            {
                Services.TryAddSingleton<HookTrace>();
                Services.AddHandlerHook(typeof(TraceCommandHook<,>));
                Services.AddHandlerHook(typeof(TraceResponseHook<,,>));
                Services.AddHandlerHook(typeof(TraceQueryHook<,>));
            }

            foreach (var configure in serviceConfigurations)
                configure(Services);

            var provider = Services.BuildServiceProvider();
            var storage = provider.GetService<InMemoryTurtlePathStorage>();

            if (storage != null)
            {
                foreach (var seed in provider.GetServices<ISeedRegistration>())
                    seed.Apply(storage);
            }

            return ValueTask.FromResult(new TurtlePathTestHost(provider));
        }

        private interface ISeedRegistration
        {
            void Apply(InMemoryTurtlePathStorage storage);
        }

        private sealed class SeedRegistration<TEntity> : ISeedRegistration
            where TEntity : class, TurtlePath.Domain.Contracts.IEntity
        {
            private readonly TEntity[] entities;

            public SeedRegistration(TEntity[] entities)
            {
                this.entities = entities ?? [];
            }

            public void Apply(InMemoryTurtlePathStorage storage)
                => storage.Seed(entities);
        }
    }
}
