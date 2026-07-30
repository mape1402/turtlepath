namespace Microsoft.Extensions.DependencyInjection
{
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Provides extension methods for configuring CId identifier services in the dependency injection container.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Configures the allowed type, default factory, and database value converter for <see cref="CId"/> identifiers in the service collection.
        /// </summary>
        /// <typeparam name="TTargetType">The type to be used for database storage of the identifier value.</typeparam>
    /// <typeparam name="TDbType">The database storage type used for the identifier value.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="setup">The action that configures the <see cref="CIdConfiguration{TTargetType, TDbType}"/> options.</param>
        public static IServiceCollection UseCId<TTargetType, TDbType>(this IServiceCollection services, Action<CIdConfiguration<TTargetType, TDbType>> setup)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setup == null)
                throw new ArgumentNullException(nameof(setup));

            var config = new CIdConfiguration<TTargetType, TDbType>();
            setup(config);

            config.ValidateAndThrow();

            CIdMetadata.AllowedType = typeof(TTargetType);
            CIdMetadata.DbType = config.DbType;
            CIdMetadata.DefaultFactory = config.DefaultFactory;
            CIdMetadata.ToByteArrayFunction = (v) => config.ToByteArrayFunction((TTargetType)v);
            CIdMetadata.JsonConverter = config.JsonConverter;
            CIdMetadata.NullableJsonConverter = config.NullableJsonConverter;
            CIdMetadata.ParseFunction = config.ParseFunction;

            var registry = new CIdDefinitionRegistry();
            registry.Register(new CIdDefinition(
                CIdDefinition.DefaultContext,
                typeof(TTargetType),
                config.DefaultFactory,
                config.ParseFunction,
                id => id.ToString(),
                id => id.ToByteArray(),
                config.GenerationStrategy));

            services.AddSingleton<ICIdDefinitionRegistry>(registry);
            services.AddSingleton<ICIdFactory>(registry);

            return services;
        }
    }
}

