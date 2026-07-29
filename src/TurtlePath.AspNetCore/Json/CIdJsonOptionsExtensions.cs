namespace TurtlePath.AspNetCore.Json
{
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Provides JSON registration helpers for TurtlePath identifiers.
    /// </summary>
    public static class CIdJsonOptionsExtensions
    {
        /// <summary>
        /// Adds TurtlePath identifier converters to JSON serializer options.
        /// </summary>
        /// <param name="options">The serializer options.</param>
        /// <returns>The same serializer options.</returns>
        public static JsonSerializerOptions AddTurtlePathCIdConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new CIdJsonConverter());
            options.Converters.Add(new CIdNulleableJsonConverter());

            return options;
        }

        /// <summary>
        /// Adds TurtlePath identifier converters to MVC JSON options.
        /// </summary>
        /// <param name="builder">The MVC builder.</param>
        /// <returns>The same MVC builder.</returns>
        public static IMvcBuilder AddTurtlePathCIdJson(this IMvcBuilder builder)
            => builder.AddJsonOptions(options => options.JsonSerializerOptions.AddTurtlePathCIdConverters());
    }
}
