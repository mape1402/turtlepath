namespace TurtlePath.EntityFrameworkCore
{
    using System.Reflection;
    using TurtlePath.Domain.Identifier;

    /// <summary>
    /// Configures the TurtlePath conventions applied by <see cref="BaseDbContext"/>.
    /// </summary>
    public sealed record TurtlePathDbContextOptions
    {
        /// <summary>
        /// Gets the default TurtlePath Entity Framework Core options.
        /// </summary>
        public static TurtlePathDbContextOptions Default { get; } = new();

        /// <summary>
        /// Gets a value indicating whether entity configurations should be applied from assemblies.
        /// </summary>
        public bool ApplyConfigurations { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether BaseEntity Id conventions should be applied.
        /// </summary>
        public bool ApplyBaseEntityConventions { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether CId value converters should be applied.
        /// </summary>
        public bool ApplyCIdConverters { get; init; } = true;

        /// <summary>
        /// Gets the identifier definition used to configure CId database conversion.
        /// </summary>
        public CIdDefinition CIdDefinition { get; init; }

        /// <summary>
        /// Gets the identifier definitions used to configure CId database conversion.
        /// </summary>
        public ICIdDefinitionRegistry CIdDefinitions { get; init; }

        /// <summary>
        /// Gets the assemblies used to discover entity configurations. When empty, the DbContext assembly is used.
        /// </summary>
        public IReadOnlyCollection<Assembly> ConfigurationAssemblies { get; init; } = Array.Empty<Assembly>();
    }
}
