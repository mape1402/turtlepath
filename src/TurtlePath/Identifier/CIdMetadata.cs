namespace TurtlePath.Identifier
{
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    /// <summary>
    /// Provides metadata and configuration for the <see cref="CId"/> identifier type.
    /// </summary>
    public static class CIdMetadata
    {
        /// <summary>
        /// Gets or sets the allowed type for the value of a <see cref="CId"/>.
        /// </summary>
        public static Type AllowedType { get; internal set; }

        /// <summary>
        /// Gets or sets the database type used by the current context.
        /// </summary>
        /// <remarks>This property is intended for internal use. Assign a value that is compatible with
        /// the underlying database system to ensure correct data processing and storage.</remarks>
        public static string DbType { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether a database type has been defined.
        /// </summary>
        /// <remarks>This property returns <see langword="true"/> if the database type is not null or
        /// empty, indicating that a valid type has been set. It can be used to validate the state before performing
        /// operations that require a defined database type.</remarks>
        public static bool HasDbType => !string.IsNullOrEmpty(DbType);

        /// <summary>
        /// Gets or sets the default factory function for creating new <see cref="CId"/> instances.
        /// </summary>
        public static Func<CId> DefaultFactory { get; internal set; } = () => new CId();

        /// <summary>
        /// Gets or sets the function that converts an instance of TTargetType to a byte array.
        /// </summary>
        /// <remarks>Use this property to specify a custom serialization function for TTargetType
        /// instances. The provided function should handle all necessary conversions and account for any edge cases
        /// relevant to your application's requirements.</remarks>
        public static Func<object, byte[]> ToByteArrayFunction { get; internal set; }

        /// <summary>
        /// Gets or sets the value converter used for database persistence of <see cref="CId"/> values.
        /// </summary>
        public static ValueConverter DbConverter { get; internal set; }

        /// <summary>
        /// Gets or sets the function used to convert a JSON string to a <see cref="CId"/> object.
        /// </summary>
        /// <remarks>This property is intended for internal use to facilitate JSON deserialization of <see
        /// cref="CId"/> objects.</remarks>
        public static Func<string, CId> JsonConverter { get; internal set; } = value => new CId(value);

        /// <summary>
        /// Gets or sets the function used to convert a JSON string to a nullable <see cref="CId"/> object.
        /// </summary>
        public static Func<string, CId?> NullableJsonConverter { get; internal set; } = value => new CId(value);

        /// <summary>
        /// Gets or sets the function used to parse a string into a <see cref="CId"/> object.
        /// </summary>
        /// <remarks>The function is used to convert a string representation into a <see cref="CId"/>
        /// instance. This property is intended for internal configuration and should be set with caution.</remarks>
        public static Func<string, CId> ParseFunction { get; internal set; } = value => new CId(value);

        /// <summary>
        /// Restores the default identifier configuration.
        /// </summary>
        public static void Reset()
        {
            AllowedType = null;
            DbType = null;
            DefaultFactory = () => new CId();
            ToByteArrayFunction = null;
            DbConverter = null;
            JsonConverter = value => new CId(value);
            NullableJsonConverter = value => new CId(value);
            ParseFunction = value => new CId(value);
        }
    }
}
