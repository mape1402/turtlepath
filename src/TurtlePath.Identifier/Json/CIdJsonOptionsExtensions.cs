namespace TurtlePath.Identifier.Json
{
    using System.Text.Json;

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
            options.Converters.Add(new CIdNullableJsonConverter());

            return options;
        }

    }
}
