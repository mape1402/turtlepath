namespace TurtlePath.Domain.Identifier
{
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
        public CIdDefinition(
            string context,
            Type valueType,
            Func<CId> factory,
            Func<string, CId> parser,
            Func<CId, string> formatter,
            Func<CId, byte[]> toByteArray,
            CIdGenerationStrategy generationStrategy)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ToByteArray = toByteArray ?? throw new ArgumentNullException(nameof(toByteArray));
            GenerationStrategy = generationStrategy;
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
    }
}

