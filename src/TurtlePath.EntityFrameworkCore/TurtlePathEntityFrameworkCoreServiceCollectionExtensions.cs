namespace Microsoft.Extensions.DependencyInjection
{
    using TurtlePath.EntityFrameworkCore;
    using TurtlePath.Domain.Identifier;
    using TurtlePath;

    /// <summary>
    /// Provides registration helpers for TurtlePath Entity Framework Core integration.
    /// </summary>
    public static class TurtlePathEntityFrameworkCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath Entity Framework Core options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">The options configuration callback.</param>
        /// <returns>The same service collection.</returns>
        public static IServiceCollection AddTurtlePathEntityFrameworkCore(
            this IServiceCollection services,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(provider =>
            {
                var options = configure?.Invoke(TurtlePathDbContextOptions.Default) ?? TurtlePathDbContextOptions.Default;

                if (options.CIdDefinition != null)
                    return options;

                var registry = provider.GetService<ICIdDefinitionRegistry>();

                if (registry == null)
                    return options;

                try
                {
                    return options with { CIdDefinition = registry.Get() };
                }
                catch (InvalidOperationException)
                {
                    return options;
                }
            });

            return services;
        }

        /// <summary>
        /// Registers TurtlePath Entity Framework Core options on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configure">The options configuration callback.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEntityFrameworkCore(
            this ITurtlePathBuilder builder,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions> configure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddTurtlePathEntityFrameworkCore(configure);

            return builder;
        }
    }
}
