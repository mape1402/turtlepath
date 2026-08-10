using System.Net;
using System.Diagnostics.CodeAnalysis;
using TurtlePath.Exceptions;
using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.AspNetCore;
using TurtlePath.ExceptionHandling.Consumers;
using TurtlePath.ExceptionHandling.Workers;
using TurtlePath.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    internal static class ExceptionHandlingExtensions
    {
        internal static IServiceCollection AddExceptionHandlingDefaults(
            this IServiceCollection services,
            Action<ExceptionHandlingOptionsBuilder> configure = null)
        {
            services.AddTemplateExceptionHandlingCore(configure);
            services.AddTurtlePathAspNetCoreExceptionHandling();
            services.AddTurtlePathConsumerExceptionHandling();

            return services;
        }

        internal static IServiceCollection AddJobExceptionHandlingDefaults(
            this IServiceCollection services,
            Action<ExceptionHandlingOptionsBuilder> configure = null)
        {
            services.AddTemplateExceptionHandlingCore(configure);
            services.AddTurtlePathWorkerExceptionHandling();

            return services;
        }

        private static IServiceCollection AddTemplateExceptionHandlingCore(
            this IServiceCollection services,
            Action<ExceptionHandlingOptionsBuilder> configure = null)
        {
            return services.AddTurtlePathExceptionHandlingCore(builder =>
            {
                builder.For<ValidationException>(
                    _ => ExceptionKind.Validation,
                    exception => "validation",
                    exception => exception.Errors);

                builder.For<HttpException>(
                    exception => MapHttpStatusCode(exception.StatusCode),
                    exception => ((int)exception.StatusCode).ToString(),
                    exception => [ exception.Message ]);

                builder.For<BadRequestException>(ExceptionKind.Validation, exception => exception.Message);
                builder.For<ForbiddenException>(ExceptionKind.Forbidden, exception => exception.Message);
                builder.For<NotFoundException>(ExceptionKind.NotFound, exception => exception.Message);
                builder.For<UnauthorizedException>(ExceptionKind.Unauthorized, exception => exception.Message);

                configure?.Invoke(builder);
            });
        }

        private static ExceptionKind MapHttpStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                HttpStatusCode.BadRequest => ExceptionKind.Validation,
                HttpStatusCode.Unauthorized => ExceptionKind.Unauthorized,
                HttpStatusCode.Forbidden => ExceptionKind.Forbidden,
                HttpStatusCode.NotFound => ExceptionKind.NotFound,
                HttpStatusCode.Conflict => ExceptionKind.Conflict,
                _ when (int)statusCode >= 500 => ExceptionKind.Transient,
                _ => ExceptionKind.Business
            };
        }
    }
}
