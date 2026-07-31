namespace TurtlePath.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore;
    using System.Reflection;
    using TurtlePath.EntityFrameworkCore.Conventions;

    /// <summary>
    /// Provides a reusable EF Core DbContext base for TurtlePath applications.
    /// </summary>
    public abstract class BaseDbContext : DbContext, IDbContext
    {
        private readonly IEnumerable<ITurtlePathModelConvention> modelConventions;
        private readonly TurtlePathDbContextOptions turtlePathOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to configure the context.</param>
        /// <param name="turtlePathOptions">The TurtlePath conventions to apply to the model.</param>
        /// <param name="modelConventions">The model conventions to apply.</param>
        protected BaseDbContext(
            DbContextOptions options,
            TurtlePathDbContextOptions turtlePathOptions,
            IEnumerable<ITurtlePathModelConvention> modelConventions) : base(options)
        {
            this.turtlePathOptions = turtlePathOptions ?? TurtlePathDbContextOptions.Default;
            this.modelConventions = modelConventions ?? Array.Empty<ITurtlePathModelConvention>();
        }

        /// <summary>
        /// Gets the assemblies used to discover entity configurations.
        /// </summary>
        protected virtual IEnumerable<Assembly> ConfigurationAssemblies
        {
            get { yield return GetType().Assembly; }
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (turtlePathOptions.ApplyConfigurations)
                ApplyConfigurations(builder);

            foreach (var convention in modelConventions)
                convention.Apply(builder, turtlePathOptions);
        }

        private void ApplyConfigurations(ModelBuilder builder)
        {
            foreach (var assembly in GetConfigurationAssemblies().Where(assembly => assembly != null).Distinct())
                builder.ApplyConfigurationsFromAssembly(assembly);
        }

        private IEnumerable<Assembly> GetConfigurationAssemblies()
            => turtlePathOptions.ConfigurationAssemblies.Count > 0
                ? turtlePathOptions.ConfigurationAssemblies
                : ConfigurationAssemblies;

    }
}
