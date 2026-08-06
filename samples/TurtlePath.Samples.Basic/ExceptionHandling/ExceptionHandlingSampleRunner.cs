using Microsoft.Extensions.DependencyInjection;
using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.AspNetCore;
using TurtlePath.ExceptionHandling.Consumers;
using TurtlePath.ExceptionHandling.Workers;
using TurtlePath.Validation;

namespace TurtlePath.Samples.Basic.ExceptionHandling;

public static class ExceptionHandlingSampleRunner
{
    public static async Task<IReadOnlyList<string>> RunAsync()
    {
        var services = new ServiceCollection();
        var reports = new SampleExceptionReportLog();

        services.AddOptions();
        services.AddLogging();
        services.AddSingleton(reports);
        services.AddSingleton<IConsumerExceptionReporter, SampleConsumerExceptionReporter>();
        services.AddSingleton<IBackgroundExceptionReporter, SampleBackgroundExceptionReporter>();

        services.AddExceptionHandlingProfile<SampleExceptionHandlingProfile>();

        services.AddTurtlePathAspNetCoreExceptionHandling(builder =>
        {
            builder.Map(ExceptionKind.Business, 422);
        });

        services.AddTurtlePathConsumerExceptionHandling(builder =>
        {
            builder.RethrowWhen((descriptor, _) => descriptor.Kind != ExceptionKind.Validation);
        });

        services.AddTurtlePathWorkerExceptionHandling(builder =>
        {
            builder.RethrowWhen(descriptor => descriptor.Kind == ExceptionKind.Transient);
            builder.Return(descriptor => $"handled:{descriptor.Code}");
        });

        using var provider = services.BuildServiceProvider();

        var lines = new List<string>();

        lines.Add(RunNeutralCoreSample(provider));
        lines.Add(RunAspNetCoreSample(provider));
        lines.Add(await RunConsumerValidationSampleAsync(provider));
        lines.Add(await RunConsumerRetrySampleAsync(provider));
        lines.Add(await RunWorkerSampleAsync(provider));
        lines.Add(await RunCronSampleAsync(provider));

        lines.AddRange(reports.Entries.Select(entry => $"report: {entry}"));

        return lines;
    }

    private static string RunNeutralCoreSample(IServiceProvider provider)
    {
        var handler = provider.GetRequiredService<IExceptionHandler>();
        var descriptor = handler.Handle(
            new SampleBusinessException("Credit limit exceeded."),
            new ExceptionHandlingContext
            {
                TraceIdentifier = "core-trace-1"
            });

        return $"core maps SampleBusinessException to kind={descriptor.Kind}, code={descriptor.Code}, trace={descriptor.TraceIdentifier}";
    }

    private static string RunAspNetCoreSample(IServiceProvider provider)
    {
        var handler = provider.GetRequiredService<IExceptionHandler>();
        var statusCodeMapper = provider.GetRequiredService<IHttpExceptionStatusCodeMapper>();
        var responseFactory = provider.GetRequiredService<IHttpExceptionResponseFactory>();

        var descriptor = handler.Handle(new SampleBusinessException("Credit limit exceeded."));
        var statusCode = statusCodeMapper.Map(descriptor);
        var response = responseFactory.Create(descriptor, statusCode);

        return $"aspnetcore projects kind={descriptor.Kind} to status={statusCode}, response={response.GetType().Name}";
    }

    private static async Task<string> RunConsumerValidationSampleAsync(IServiceProvider provider)
    {
        var boundary = provider.GetRequiredService<IConsumerExceptionBoundary>();

        await boundary.RunAsync(
            new CustomerImported("bad-email"),
            (_, _) => throw new ValidationException([ "Email is invalid." ]),
            new ConsumerExceptionContext
            {
                MessageId = "message-validation",
                CorrelationId = "correlation-validation",
                DeliveryCount = 1
            });
          
        return "consumer completes validation errors without rethrowing";
    }

    private static async Task<string> RunConsumerRetrySampleAsync(IServiceProvider provider)
    {
        var boundary = provider.GetRequiredService<IConsumerExceptionBoundary>();

        try
        {
            await boundary.RunAsync(
                new CustomerImported("ada@turtlepath.dev"),
                (_, _) => throw new SampleTransientException("Broker dependency unavailable."),
                new ConsumerExceptionContext
                {
                    MessageId = "message-transient",
                    CorrelationId = "correlation-transient",
                    DeliveryCount = 2
                });
        }
        catch (SampleTransientException)
        {
            return "consumer rethrows transient errors so the broker can retry or dead-letter";
        }

        return "consumer transient sample completed";
    }

    private static async Task<string> RunWorkerSampleAsync(IServiceProvider provider)
    {
        var boundary = provider.GetRequiredService<IBackgroundExceptionBoundary>();

        var result = await boundary.RunAsync<string>(
            _ => throw new SampleBusinessException("Cache warmup skipped."),
            new BackgroundExceptionContext
            {
                Workload = "cache-warmup",
                TraceIdentifier = "worker-trace-1"
            });

        return $"worker completes business errors with fallback={result}";
    }

    private static async Task<string> RunCronSampleAsync(IServiceProvider provider)
    {
        var boundary = provider.GetRequiredService<IBackgroundExceptionBoundary>();

        try
        {
            await boundary.RunAsync(
                _ => throw new SampleTransientException("Database unavailable."),
                new BackgroundExceptionContext
                {
                    Workload = "nightly-import",
                    TraceIdentifier = "cron-trace-1"
                });
        }
        catch (SampleTransientException)
        {
            return "cron rethrows transient errors so Kubernetes marks the job as failed";
        }

        return "cron transient sample completed";
    }

    private sealed record CustomerImported(string Email);
}
