namespace TurtlePath.AutoMapper
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Mapping;

    /// <summary>
    /// Provides AutoMapper registration helpers for TurtlePath.
    /// </summary>
    public static class AutoMapperTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Registers the AutoMapper mapper adapter on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseAutoMapper(this ITurtlePathBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<IMapperAdapter, AutoMapperAdapter>();

            return builder;
        }
    }
}
