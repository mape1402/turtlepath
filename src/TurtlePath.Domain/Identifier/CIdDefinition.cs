namespace TurtlePath.Domain.Identifier
{
    using System.Linq.Expressions;

    /// <summary>
    /// Describes how an opaque identifier is created, parsed, and formatted in a specific context.
    /// </summary>
    public sealed class CIdDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CIdDefinition"/> class.
        /// </summary>
        /// <param name="context">The definition context.</param>
        /// <param name="valueType">The CLR value type represented by this definition.</param>
        /// <param name="factory">The factory used for client-generated identifiers.</param>
        /// <param name="parser">The parser used to create identifiers from text.</param>
        /// <param name="formatter">The formatter used to convert identifiers to text.</param>
        /// <param name="toByteArray">The converter used to expose the identifier as bytes.</param>
        /// <param name="generationStrategy">The identifier generation strategy.</param>
        /// <param name="databaseValueType">The provider CLR type used to store this identifier in a database.</param>
        /// <param name="databaseColumnType">The database column type used to store this identifier.</param>
        /// <param name="convertToDatabase">The converter from CId to database value.</param>
        /// <param name="convertFromDatabase">The converter from database value to CId.</param>
        public CIdDefinition(
            string context,
            Type valueType,
            Func<CId> factory,
            Func<string, CId> parser,
            Func<CId, string> formatter,
            Func<CId, byte[]> toByteArray,
            CIdGenerationStrategy generationStrategy,
            Type databaseValueType = null,
            string databaseColumnType = null,
            LambdaExpression convertToDatabase = null,
            LambdaExpression convertFromDatabase = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ToByteArray = toByteArray ?? throw new ArgumentNullException(nameof(toByteArray));
            GenerationStrategy = generationStrategy;
            DatabaseValueType = databaseValueType;
            DatabaseColumnType = databaseColumnType;
            ConvertToDatabase = convertToDatabase;
            ConvertFromDatabase = convertFromDatabase;
        }

        /// <summary>
        /// Gets the default definition context.
        /// </summary>
        public const string DefaultContext = "default";

        /// <summary>
        /// Gets the definition context.
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Gets the CLR value type represented by this definition.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Gets the identifier factory.
        /// </summary>
        public Func<CId> Factory { get; }

        /// <summary>
        /// Gets the parser.
        /// </summary>
        public Func<string, CId> Parser { get; }

        /// <summary>
        /// Gets the formatter.
        /// </summary>
        public Func<CId, string> Formatter { get; }

        /// <summary>
        /// Gets the byte converter.
        /// </summary>
        public Func<CId, byte[]> ToByteArray { get; }

        /// <summary>
        /// Gets the generation strategy.
        /// </summary>
        public CIdGenerationStrategy GenerationStrategy { get; }

        /// <summary>
        /// Gets the provider CLR type used to store this identifier in a database.
        /// </summary>
        public Type DatabaseValueType { get; }

        /// <summary>
        /// Gets the database column type used to store this identifier.
        /// </summary>
        public string DatabaseColumnType { get; }

        /// <summary>
        /// Gets the expression that converts a CId to its database value.
        /// </summary>
        public LambdaExpression ConvertToDatabase { get; }

        /// <summary>
        /// Gets the expression that converts a database value to a CId.
        /// </summary>
        public LambdaExpression ConvertFromDatabase { get; }

        /// <summary>
        /// Gets a value indicating whether this definition has database conversion metadata.
        /// </summary>
        public bool HasDatabaseConversion => ConvertToDatabase != null && ConvertFromDatabase != null;
    }
}

