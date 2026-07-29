namespace TurtlePath.AspNetCore.Json
{
    using TurtlePath.Identifier;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Provides JSON conversion for nullable <see cref="CId"/> values.
    /// </summary>
    public sealed class CIdNulleableJsonConverter : JsonConverter<CId?>
    {
        /// <summary>
        /// Reads a nullable <see cref="CId"/> value from JSON.
        /// </summary>
        /// <param name="reader">The JSON reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The parsed <see cref="CId"/> value.</returns>
        public override CId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return CIdMetadata.NullableJsonConverter(value);
        }

        /// <summary>
        /// Writes a nullable <see cref="CId"/> value to JSON.
        /// </summary>
        /// <param name="writer">The JSON writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, CId? value, JsonSerializerOptions options)
        {
            var writeValue = value is null ? null : value.ToString();
            writer.WriteStringValue(writeValue);
        }
    }
}
