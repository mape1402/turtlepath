using TurtlePath.ExceptionHandling;
using TurtlePath.Exceptions;
using TurtlePath.Validation;

namespace TurtlePath.Samples.Basic.ExceptionHandling;

public sealed class SampleExceptionHandlingProfile : ExceptionHandlingProfile
{
    public override void Configure(ExceptionHandlingOptionsBuilder builder)
    {
        builder.For<ValidationException>(
            _ => ExceptionKind.Validation,
            _ => "validation",
            exception => exception.Errors);

        builder.For<SampleBusinessException>(ExceptionKind.Business, exception => exception.Message);
        builder.For<SampleTransientException>(ExceptionKind.Transient, exception => exception.Message);
        builder.For<HttpException>(
            exception => MapHttpStatusCode(exception.StatusCode),
            exception => ((int)exception.StatusCode).ToString(),
            exception => [ exception.Message ]);
    }

    private static ExceptionKind MapHttpStatusCode(System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => ExceptionKind.Validation,
            System.Net.HttpStatusCode.Unauthorized => ExceptionKind.Unauthorized,
            System.Net.HttpStatusCode.Forbidden => ExceptionKind.Forbidden,
            System.Net.HttpStatusCode.NotFound => ExceptionKind.NotFound,
            System.Net.HttpStatusCode.Conflict => ExceptionKind.Conflict,
            _ when (int)statusCode >= 500 => ExceptionKind.Transient,
            _ => ExceptionKind.Business
        };
    }
}
