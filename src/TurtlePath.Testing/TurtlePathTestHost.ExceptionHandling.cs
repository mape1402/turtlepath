namespace TurtlePath.Testing
{
    using TurtlePath.ExceptionHandling;
    using TurtlePath.Testing.ExceptionHandling;

    public sealed partial class TurtlePathTestHost
    {
        /// <summary>
        /// Resolves an exception through the configured TurtlePath exception handler.
        /// </summary>
        public ExceptionHandlingTestResult HandleException(
            Exception exception,
            ExceptionHandlingContext context = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var descriptor = Resolve<IExceptionHandler>().Handle(exception, context);

            return new ExceptionHandlingTestResult(exception, descriptor);
        }
    }
}
