namespace TurtlePath.ExceptionHandling
{
    /// <summary>
    /// Provides fluent configuration for exception handling options.
    /// </summary>
    public sealed class ExceptionHandlingOptionsBuilder
    {
        private readonly ExceptionHandlingOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingOptionsBuilder"/> class.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        public ExceptionHandlingOptionsBuilder(ExceptionHandlingOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Registers an exception mapping.
        /// </summary>
        public ExceptionHandlingOptionsBuilder For<TException>(
            ExceptionKind kind,
            Func<TException, string> messageFactory)
            where TException : Exception
        {
            if (messageFactory == null)
                throw new ArgumentNullException(nameof(messageFactory));

            return For<TException>(_ => kind, exception => kind.Value, exception => new[] { messageFactory(exception) });
        }

        /// <summary>
        /// Registers an exception mapping.
        /// </summary>
        public ExceptionHandlingOptionsBuilder For<TException>(
            ExceptionKind kind,
            Func<TException, IEnumerable<string>> messageFactory)
            where TException : Exception
            => For<TException>(_ => kind, exception => kind.Value, messageFactory);

        /// <summary>
        /// Registers an exception mapping.
        /// </summary>
        public ExceptionHandlingOptionsBuilder For<TException>(
            Func<TException, ExceptionKind> kindFactory,
            Func<TException, string> messageFactory)
            where TException : Exception
        {
            if (messageFactory == null)
                throw new ArgumentNullException(nameof(messageFactory));

            return For<TException>(kindFactory, exception => kindFactory(exception)?.Value, exception => new[] { messageFactory(exception) });
        }

        /// <summary>
        /// Registers an exception mapping.
        /// </summary>
        public ExceptionHandlingOptionsBuilder For<TException>(
            Func<TException, ExceptionKind> kindFactory,
            Func<TException, IEnumerable<string>> messageFactory)
            where TException : Exception
            => For<TException>(kindFactory, exception => kindFactory(exception)?.Value, messageFactory);

        /// <summary>
        /// Registers an exception mapping.
        /// </summary>
        public ExceptionHandlingOptionsBuilder For<TException>(
            Func<TException, ExceptionKind> kindFactory,
            Func<TException, string> codeFactory,
            Func<TException, IEnumerable<string>> messageFactory,
            Func<TException, IReadOnlyDictionary<string, object>> metadataFactory = null)
            where TException : Exception
        {
            if (kindFactory == null)
                throw new ArgumentNullException(nameof(kindFactory));

            if (codeFactory == null)
                throw new ArgumentNullException(nameof(codeFactory));

            if (messageFactory == null)
                throw new ArgumentNullException(nameof(messageFactory));

            options.Rules[typeof(TException)] = new ExceptionHandlingRule(
                typeof(TException),
                exception => kindFactory((TException)exception),
                exception => codeFactory((TException)exception),
                exception => messageFactory((TException)exception) ?? Array.Empty<string>(),
                exception => metadataFactory?.Invoke((TException)exception) ?? new Dictionary<string, object>());

            return this;
        }
    }
}
