using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TurtlePath.ExceptionHandling;

namespace TurtlePath.ExceptionHandling.AspNetCore
{
    /// <summary>
    /// Handles unhandled MVC exceptions using TurtlePath exception handling.
    /// </summary>
    public sealed class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> logger;
        private readonly IExceptionHandler exceptionHandler;
        private readonly IHttpExceptionResponseFactory responseFactory;
        private readonly IHttpExceptionStatusCodeMapper statusCodeMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionFilter"/> class.
        /// </summary>
        public GlobalExceptionFilter(
            ILogger<GlobalExceptionFilter> logger,
            IExceptionHandler exceptionHandler,
            IHttpExceptionResponseFactory responseFactory,
            IHttpExceptionStatusCodeMapper statusCodeMapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            this.responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
            this.statusCodeMapper = statusCodeMapper ?? throw new ArgumentNullException(nameof(statusCodeMapper));
        }

        /// <inheritdoc />
        public void OnException(ExceptionContext context)
        {
            logger.LogError(context.Exception, context.Exception.Message);

            var descriptor = exceptionHandler.Handle(
                context.Exception,
                new ExceptionHandlingContext
                {
                    TraceIdentifier = context.HttpContext.TraceIdentifier
                });

            var statusCode = statusCodeMapper.Map(descriptor);
            var response = responseFactory.Create(descriptor, statusCode);

            context.HttpContext.Response.StatusCode = statusCode;
            context.Result = new ObjectResult(response)
            {
                StatusCode = statusCode,
                DeclaredType = response?.GetType()
            };
        }
    }
}
