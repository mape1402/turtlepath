namespace TurtlePath.Crabalidator
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides Crabalidator registration helpers for TurtlePath.
    /// </summary>
    public static class CrabalidatorTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Registers the Crabalidator validator adapter on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseCrabalidator(this ITurtlePathBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<IValidatorAdapter, CrabalidatorAdapter>();

            return builder;
        }
    }
}
