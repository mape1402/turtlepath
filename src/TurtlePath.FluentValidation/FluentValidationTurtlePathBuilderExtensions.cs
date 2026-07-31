namespace TurtlePath.FluentValidation
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides FluentValidation registration helpers for TurtlePath.
    /// </summary>
    public static class FluentValidationTurtlePathBuilderExtensions
    {
        /// <summary>
        /// Registers the FluentValidation validator adapter on the current TurtlePath pipeline.
        /// </summary>
        /// <param name="builder">The TurtlePath builder.</param>
        /// <returns>The same TurtlePath builder.</returns>
        public static ITurtlePathBuilder UseFluentValidation(this ITurtlePathBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddSingleton<IValidatorAdapter, ValidatorAdapter>();

            return builder;
        }
    }
}
