namespace Microsoft.Extensions.DependencyInjection
{
    using TurtlePath.EntityFrameworkCore;
    using TurtlePath.Domain.Identifier;
    using TurtlePath;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Provides registration helpers for TurtlePath Entity Framework Core integration.
    /// </summary>
    public static class TurtlePathEntityFrameworkCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Registers TurtlePath Entity Framework Core options and maps the concrete context to <see cref="IDbContext"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The application's DbContext type.</typeparam>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <param name="configure">The options configuration callback.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseEntityFrameworkCore<TDbContext>(
            this ITurtlePathBuilder builder,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions> configure = null)
            where TDbContext : DbContext, IDbContext
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            AddTurtlePathEntityFrameworkCore(builder.Services, configure);
            builder.Services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TDbContext>());

            return builder;
        }

        private static void AddTurtlePathEntityFrameworkCore(
            IServiceCollection services,
            Func<TurtlePathDbContextOptions, TurtlePathDbContextOptions> configure)
        {
            services.AddSingleton(provider =>
            {
                var options = configure?.Invoke(TurtlePathDbContextOptions.Default) ?? TurtlePathDbContextOptions.Default;

                if (options.CIdDefinitions != null)
                    return options;

                var registry = provider.GetService<ICIdDefinitionRegistry>();

                if (registry == null)
                    return options;

                return options with
                {
                    CIdDefinition = TryGetDefaultDefinition(registry),
                    CIdDefinitions = registry
                };
            });
        }

        private static CIdDefinition TryGetDefaultDefinition(ICIdDefinitionRegistry registry)
        {
            try
            {
                return registry.Get();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
