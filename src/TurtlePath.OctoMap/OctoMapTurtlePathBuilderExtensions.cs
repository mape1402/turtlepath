namespace TurtlePath.OctoMap
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Mapping;

    /// <summary>
    /// Provides OctoMap registration helpers for TurtlePath.
    /// </summary>
    public static class OctoMapTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Registers the OctoMap mapper adapter on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseOctoMap(this ITurtlePathBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<IMapperAdapter, MapperAdapter>();

            return builder;
        }
    }
}
