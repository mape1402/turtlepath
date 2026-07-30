namespace TurtlePath.Identifier
{
    using System.Linq.Expressions;

    /// <summary>
    /// Provides configuration options for <see cref="CId"/> identifier mapping and conversion to and from a database type.
    /// </summary>
    /// <typeparam name="TTargetType">The type used for the identifier in the domain model.</typeparam>
    /// <typeparam name="TDbType">The type used for the identifier in the database.</typeparam>
    public class CIdConfiguration<TTargetType, TDbType>
    {
        /// <summary>
        /// Gets or sets the default factory function for creating new <see cref="CId"/> instances.
        /// </summary>
        public Func<CId> DefaultFactory { get; set; }

        /// <summary>
        /// Gets or sets the database type used by the current context.
        /// </summary>
        /// <remarks>This property is intended for internal use. Assign a value that is compatible with
        /// the underlying database system to ensure correct data processing and storage.</remarks>
        public string DbType { get; set; }

        /// <summary>
        /// Gets or sets the function that converts an instance of TTargetType to a byte array.
        /// </summary>
        /// <remarks>Use this property to specify a custom serialization function for TTargetType
        /// instances. The provided function should handle all necessary conversions and account for any edge cases
        /// relevant to your application's requirements.</remarks>
        public Func<TTargetType, byte[]> ToByteArrayFunction { get; set; }

        /// <summary>
        /// Gets or sets the expression to convert a <see cref="CId"/> to the database type <typeparamref name="TDbType"/>.
        /// </summary>
        public Expression<Func<CId, TDbType>> ConvertToDb { get; set; }

        /// <summary>
        /// Gets or sets the expression to convert the database type <typeparamref name="TDbType"/> to a <see cref="CId"/>.
        /// </summary>
        public Expression<Func<TDbType, CId>> ConvertFromDb { get; set; }

        /// <summary>
        /// Gets or sets the function used to convert a JSON string to a <see cref="CId"/> object.
        /// </summary>
        /// <remarks>This property is intended for internal use to facilitate JSON deserialization of <see
        /// cref="CId"/> objects.</remarks>
        public Func<string, CId> JsonConverter { get; set; }

        /// <summary>
        /// Gets or sets the function used to convert a JSON string to a nullable <see cref="CId"/> object.
        /// </summary>
        public Func<string, CId?> NullableJsonConverter { get; set; }

        /// <summary>
        /// Gets or sets the function used to parse a string into a <see cref="CId"/> object.
        /// </summary>
        /// <remarks>The function is used to convert a string representation into a <see cref="CId"/>
        /// instance. This property is intended for internal configuration and should be set with caution.</remarks>
        public Func<string, CId> ParseFunction { get; set; }

        /// <summary>
        /// Gets or sets where identifier values are generated.
        /// </summary>
        public CIdGenerationStrategy GenerationStrategy { get; set; } = CIdGenerationStrategy.ClientGenerated;

        internal void ValidateAndThrow()
        {
            if(ConvertToDb == null)
                throw new InvalidOperationException("ConvertToDb must be set.");

            if(ConvertFromDb == null)
                throw new InvalidOperationException("ConvertFromDb must be set.");

            if(JsonConverter == null)
                throw new InvalidOperationException("JsonConverter must be set.");

            if(NullableJsonConverter == null)
                throw new InvalidOperationException("NullableJsonConverter must be set.");

            if(DefaultFactory == null)
                throw new InvalidOperationException("DefaultFactory must be set.");

            if(ParseFunction == null)
                throw new InvalidOperationException("ParseFunction must be set.");
        }
    }
}
