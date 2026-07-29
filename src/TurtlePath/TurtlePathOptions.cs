namespace TurtlePath
{
    using System.Reflection;

    /// <summary>
    /// Configures the default TurtlePath composition package registrations.
    /// </summary>
    public sealed class TurtlePathOptions
    {
        private readonly List<Assembly> _applicationAssemblies = new();

        /// <summary>
        /// Gets the assemblies scanned for application validators, maps, and handler hooks.
        /// </summary>
        public IReadOnlyCollection<Assembly> ApplicationAssemblies => _applicationAssemblies;

        /// <summary>
        /// Adds assemblies that contain application validators, maps, or handler hooks.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>The current options instance.</returns>
        public TurtlePathOptions AddApplicationAssemblies(params Assembly[] assemblies)
        {
            if (assemblies is null)
                return this;

            foreach (var assembly in assemblies.Where(assembly => assembly is not null))
            {
                if (!_applicationAssemblies.Contains(assembly))
                    _applicationAssemblies.Add(assembly);
            }

            return this;
        }
    }
}
