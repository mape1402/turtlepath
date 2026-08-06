namespace TurtlePath.ExceptionHandling
{
    internal sealed class ExceptionHandlingRule
    {
        public ExceptionHandlingRule(
            Type exceptionType,
            Func<Exception, ExceptionKind> kindFactory,
            Func<Exception, string> codeFactory,
            Func<Exception, IEnumerable<string>> messageFactory,
            Func<Exception, IReadOnlyDictionary<string, object>> metadataFactory)
        {
            ExceptionType = exceptionType ?? throw new ArgumentNullException(nameof(exceptionType));
            KindFactory = kindFactory ?? throw new ArgumentNullException(nameof(kindFactory));
            CodeFactory = codeFactory ?? throw new ArgumentNullException(nameof(codeFactory));
            MessageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
            MetadataFactory = metadataFactory ?? throw new ArgumentNullException(nameof(metadataFactory));
        }

        public Type ExceptionType { get; }

        public Func<Exception, ExceptionKind> KindFactory { get; }

        public Func<Exception, string> CodeFactory { get; }

        public Func<Exception, IEnumerable<string>> MessageFactory { get; }

        public Func<Exception, IReadOnlyDictionary<string, object>> MetadataFactory { get; }
    }
}
