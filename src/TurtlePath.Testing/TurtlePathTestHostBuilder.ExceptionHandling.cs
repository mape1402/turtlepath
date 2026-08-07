namespace TurtlePath.Testing
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.ExceptionHandling;

    public sealed partial class TurtlePathTestHostBuilder
    {
        /// <summary>
        /// Registers TurtlePath exception handling core for the test host.
        /// </summary>
        public TurtlePathTestHostBuilder UseExceptionHandling(
            Action<ExceptionHandlingOptionsBuilder> configure = null)
            => ConfigureServices(services => services.AddTurtlePathExceptionHandlingCore(configure));
    }
}
