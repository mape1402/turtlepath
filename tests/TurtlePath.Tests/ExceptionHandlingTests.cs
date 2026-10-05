using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TurtlePath.ExceptionHandling;
using TurtlePath.ExceptionHandling.AspNetCore;
using TurtlePath.ExceptionHandling.Consumers;
using TurtlePath.ExceptionHandling.Workers;

namespace TurtlePath.Tests;

public sealed class ExceptionHandlingTests
{
    [Fact]
    public void Handler_resolves_registered_exception_descriptor()
    {
        var handler = CreateHandler(builder =>
        {
            builder.For<SampleValidationException>(
                _ => ExceptionKind.Validation,
                exception => "sample_validation",
                exception => exception.Errors,
                exception => new Dictionary<string, object>
                {
                    ["field"] = exception.Field
                });
        });

        var descriptor = handler.Handle(
            new SampleValidationException("name", "Name is required.", "Name is too short."),
            new ExceptionHandlingContext
            {
                TraceIdentifier = "trace-1"
            });

        Assert.Equal(ExceptionKind.Validation, descriptor.Kind);
        Assert.Equal(ExceptionKind.Validation.Value, descriptor.Kind.ToString());
        Assert.Equal("sample_validation", descriptor.Code);
        Assert.Equal(new[] { "Name is required.", "Name is too short." }, descriptor.Messages);
        Assert.Equal("name", descriptor.Metadata["field"]);
        Assert.Equal("trace-1", descriptor.TraceIdentifier);
    }

    [Fact]
    public void Handler_uses_exact_exception_type_mapping_only()
    {
        var handler = CreateHandler(builder =>
        {
            builder.For<Exception>(ExceptionKind.Business, exception => exception.Message);
        });

        var descriptor = handler.Handle(new InvalidOperationException("Derived failure."));

        Assert.Equal(ExceptionKind.Failure, descriptor.Kind);
        Assert.Equal(ExceptionKind.Failure.Value, descriptor.Code);
        Assert.Equal(new[] { "Derived failure." }, descriptor.Messages);
    }

    [Fact]
    public void Services_register_core_and_aspnetcore_adapters()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePathExceptionHandlingCore(builder =>
            {
                builder.For<InvalidOperationException>(ExceptionKind.Conflict, exception => exception.Message);
            })
            .AddTurtlePathAspNetCoreExceptionHandling(builder =>
            {
                builder.Map(ExceptionKind.Conflict, StatusCodes.Status409Conflict);
            });

        using var provider = services.BuildServiceProvider();

        var handler = provider.GetRequiredService<IExceptionHandler>();
        var mapper = provider.GetRequiredService<IHttpExceptionStatusCodeMapper>();
        var descriptor = handler.Handle(new InvalidOperationException("Conflict."));

        Assert.Equal(StatusCodes.Status409Conflict, mapper.Map(descriptor));
        Assert.NotNull(provider.GetRequiredService<IHttpExceptionResponseFactory>());
    }

    [Fact]
    public void Services_register_exception_handling_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services.AddExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IExceptionHandler>();
        var descriptor = handler.Handle(new ProfileMappedException("Profile mapped."));

        Assert.Equal(ProfileMappedExceptionKind, descriptor.Kind);
        Assert.Equal("profile_mapped", descriptor.Code);
        Assert.Equal(new[] { "Profile mapped." }, descriptor.Messages);
    }

    [Fact]
    public void Services_register_http_exception_handling_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services
            .AddTurtlePathAspNetCoreExceptionHandling()
            .AddHttpExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IHttpExceptionStatusCodeMapper>();

        Assert.Equal(
            StatusCodes.Status403Forbidden,
            mapper.Map(new ExceptionDescriptor { Kind = ProfileMappedExceptionKind }));
    }

    [Fact]
    public void Services_register_consumer_exception_handling_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services
            .AddLogging()
            .AddTurtlePathConsumerExceptionHandling()
            .AddConsumerExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ConsumerExceptionHandlingOptions>>().Value;

        Assert.False(options.ShouldRethrow(
            new ExceptionDescriptor { Kind = ProfileMappedExceptionKind },
            new ConsumerExceptionContext()));
        Assert.True(options.ShouldRethrow(
            new ExceptionDescriptor { Kind = ExceptionKind.Failure },
            new ConsumerExceptionContext()));
    }

    [Fact]
    public void Services_register_background_exception_handling_profiles_from_assemblies()
    {
        var services = new ServiceCollection();

        services
            .AddLogging()
            .AddTurtlePathWorkerExceptionHandling()
            .AddBackgroundExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<BackgroundExceptionHandlingOptions>>().Value;

        Assert.False(options.ShouldRethrow(new ExceptionDescriptor { Kind = ProfileMappedExceptionKind }));
        Assert.True(options.ShouldRethrow(new ExceptionDescriptor { Kind = ExceptionKind.Failure }));
    }

    [Fact]
    public void Problem_details_factory_projects_descriptor()
    {
        var factory = new ProblemDetailsExceptionResponseFactory(Options.Create(new ApiBehaviorOptions()));
        var response = Assert.IsType<ProblemDetails>(factory.Create(
            new ExceptionDescriptor
            {
                Kind = ExceptionKind.Validation,
                Code = "validation",
                Messages = new[] { "Name is required." },
                Metadata = new Dictionary<string, object>
                {
                    ["field"] = "name"
                },
                TraceIdentifier = "trace-2"
            },
            StatusCodes.Status400BadRequest));

        Assert.Equal(StatusCodes.Status400BadRequest, response.Status);
        Assert.Equal("Name is required.", response.Detail);
        Assert.Equal("trace-2", response.Instance);
        Assert.Equal("validation", response.Extensions["code"]);
        Assert.Equal(ExceptionKind.Validation.Value, response.Extensions["kind"]);
        Assert.Equal("name", response.Extensions["field"]);
    }

    [Fact]
    public void Problem_details_factory_uses_client_error_mapping_and_fallbacks()
    {
        var options = new ApiBehaviorOptions();
        options.ClientErrorMapping[499] = new ClientErrorData
        {
            Link = "https://example.test/client-error",
            Title = "Client closed request"
        };
        var mappedFactory = new ProblemDetailsExceptionResponseFactory(Options.Create(options));
        var fallbackFactory = new ProblemDetailsExceptionResponseFactory(null);

        var mapped = Assert.IsType<ProblemDetails>(mappedFactory.Create(new ExceptionDescriptor
        {
            Kind = ExceptionKind.Business,
            Code = "client_closed",
            Messages = []
        }, 499));
        var fallback = Assert.IsType<ProblemDetails>(fallbackFactory.Create(new ExceptionDescriptor
        {
            Kind = ExceptionKind.Failure,
            Code = "unknown",
            Messages = []
        }, 599));

        Assert.Equal("https://example.test/client-error", mapped.Type);
        Assert.Equal("Client closed request", mapped.Title);
        Assert.Equal("An unexpected error occurred.", fallback.Title);
        Assert.Throws<ArgumentNullException>(() => fallbackFactory.Create(null, StatusCodes.Status500InternalServerError));
    }

    [Fact]
    public void Global_exception_filter_maps_exception_to_http_response()
    {
        var handler = CreateHandler(builder =>
        {
            builder.For<InvalidOperationException>(ExceptionKind.Conflict, exception => exception.Message);
        });
        var mapper = new DefaultHttpExceptionStatusCodeMapper(Options.Create(new HttpExceptionHandlingOptions
        {
            StatusCodeMappings =
            {
                [ExceptionKind.Conflict] = StatusCodes.Status409Conflict
            }
        }));
        var fallbackMapper = new DefaultHttpExceptionStatusCodeMapper(Options.Create(new HttpExceptionHandlingOptions()));
        var factory = new ProblemDetailsExceptionResponseFactory(Options.Create(new ApiBehaviorOptions()));
        var filter = new GlobalExceptionFilter(
            NullLogger<GlobalExceptionFilter>.Instance,
            handler,
            factory,
            mapper);
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-filter"
        };
        var context = new ExceptionContext(
            new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
            [])
        {
            Exception = new InvalidOperationException("Broken.")
        };

        filter.OnException(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
        Assert.Equal(typeof(ProblemDetails), result.DeclaredType);
        Assert.Equal("Broken.", problem.Detail);
        Assert.Equal("trace-filter", problem.Instance);
        Assert.Equal(StatusCodes.Status500InternalServerError, fallbackMapper.Map(new ExceptionDescriptor { Kind = ExceptionKind.Failure }));
        Assert.Throws<ArgumentNullException>(() => mapper.Map(null));
    }

    [Fact]
    public async Task Background_boundary_can_complete_handled_exceptions()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services
            .AddTurtlePathExceptionHandlingCore(builder =>
            {
                builder.For<InvalidOperationException>(ExceptionKind.Business, exception => exception.Message);
            })
            .AddTurtlePathWorkerExceptionHandling(builder =>
            {
                builder.Complete();
                builder.Return(_ => "fallback");
            });

        using var provider = services.BuildServiceProvider();
        var boundary = provider.GetRequiredService<IBackgroundExceptionBoundary>();

        await boundary.RunAsync(_ => throw new InvalidOperationException("Worker failed."));
        var result = await boundary.RunAsync<string>(_ => throw new InvalidOperationException("Worker failed."));

        Assert.Equal("fallback", result);
    }

    [Fact]
    public async Task Background_boundary_rethrows_by_default_for_cron_jobs()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services
            .AddTurtlePathExceptionHandlingCore()
            .AddTurtlePathWorkerExceptionHandling();

        using var provider = services.BuildServiceProvider();
        var boundary = provider.GetRequiredService<IBackgroundExceptionBoundary>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boundary.RunAsync(_ => throw new InvalidOperationException("Cron failed.")));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boundary.RunAsync<string>(_ => throw new InvalidOperationException("Cron failed.")));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RunAsync((Func<CancellationToken, Task>)null));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RunAsync<string>(null));
    }

    [Fact]
    public async Task Consumer_boundary_can_complete_handled_exceptions()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services
            .AddTurtlePathExceptionHandlingCore(builder =>
            {
                builder.For<InvalidOperationException>(ExceptionKind.Business, exception => exception.Message);
            })
            .AddTurtlePathConsumerExceptionHandling(builder =>
            {
                builder.Complete();
            });

        using var provider = services.BuildServiceProvider();
        var boundary = provider.GetRequiredService<IConsumerExceptionBoundary>();

        await boundary.RunAsync(
            new SampleMessage(),
            (_, _) => throw new InvalidOperationException("Consumer failed."),
            new ConsumerExceptionContext
            {
                MessageId = "message-1",
                CorrelationId = "correlation-1",
                DeliveryCount = 2
            });
    }

    [Fact]
    public async Task Consumer_boundary_rethrows_by_default_for_broker_retry()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services
            .AddTurtlePathExceptionHandlingCore()
            .AddTurtlePathConsumerExceptionHandling();

        using var provider = services.BuildServiceProvider();
        var boundary = provider.GetRequiredService<IConsumerExceptionBoundary>();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            boundary.RunAsync(_ => throw new InvalidOperationException("Consumer failed.")));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RunAsync((Func<CancellationToken, Task>)null));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RunAsync(new SampleMessage(), null));
    }

    [Fact]
    public void Exception_handling_builders_validate_arguments_and_support_all_overloads()
    {
        var options = new ExceptionHandlingOptions();
        var builder = new ExceptionHandlingOptionsBuilder(options);

        Assert.Throws<ArgumentNullException>(() => new ExceptionHandlingOptionsBuilder(null));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>(ExceptionKind.Business, (Func<InvalidOperationException, string>)null));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>((Func<InvalidOperationException, ExceptionKind>)null, exception => exception.Message));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>(_ => ExceptionKind.Business, (Func<InvalidOperationException, string>)null));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>(_ => ExceptionKind.Business, null, _ => []));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>(_ => ExceptionKind.Business, _ => "code", null));
        Assert.Throws<ArgumentNullException>(() => builder.For<InvalidOperationException>(ExceptionKind.Business, (Func<InvalidOperationException, IEnumerable<string>>)null));

        builder
            .For<ArgumentException>(ExceptionKind.Validation, exception => [exception.Message])
            .For<InvalidOperationException>(_ => ExceptionKind.Business, exception => [exception.Message])
            .For<NotSupportedException>(
                _ => ExceptionKind.Failure,
                _ => "not_supported",
                _ => null,
                _ => null);

        var handler = new DefaultExceptionHandler(Options.Create(options));
        var argument = handler.Handle(new ArgumentException("bad"));
        var invalid = handler.Handle(new InvalidOperationException("business"));
        var notSupported = handler.Handle(new NotSupportedException());
        var rules = (System.Collections.IDictionary)typeof(ExceptionHandlingOptions)
            .GetProperty("Rules", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(options);
        var rule = rules[typeof(ArgumentException)];

        Assert.Equal(ExceptionKind.Validation, argument.Kind);
        Assert.Equal(
            typeof(ArgumentException),
            rule.GetType().GetProperty("ExceptionType")!.GetValue(rule));
        Assert.Equal(["bad"], argument.Messages);
        Assert.Equal(ExceptionKind.Business, invalid.Kind);
        Assert.Equal(["business"], invalid.Messages);
        Assert.Equal("not_supported", notSupported.Code);
        Assert.Empty(notSupported.Messages);
        Assert.Empty(notSupported.Metadata);
    }

    [Fact]
    public void Transport_exception_handling_builders_validate_arguments()
    {
        var consumerOptions = new ConsumerExceptionHandlingOptions();
        var consumerBuilder = new ConsumerExceptionHandlingOptionsBuilder(consumerOptions);
        var backgroundOptions = new BackgroundExceptionHandlingOptions();
        var backgroundBuilder = new BackgroundExceptionHandlingOptionsBuilder(backgroundOptions);

        Assert.Throws<ArgumentNullException>(() => new ConsumerExceptionHandlingOptionsBuilder(null));
        Assert.Throws<ArgumentNullException>(() => consumerBuilder.HandleWhen(null));
        Assert.Throws<ArgumentNullException>(() => consumerBuilder.RethrowWhen(null));
        Assert.Throws<ArgumentNullException>(() => new BackgroundExceptionHandlingOptionsBuilder(null));
        Assert.Throws<ArgumentNullException>(() => backgroundBuilder.HandleWhen(null));
        Assert.Throws<ArgumentNullException>(() => backgroundBuilder.RethrowWhen(null));
        Assert.Throws<ArgumentNullException>(() => backgroundBuilder.Return(null));

        consumerBuilder
            .HandleWhen((exception, context, token) => context.DeliveryCount > 1)
            .Rethrow()
            .Complete()
            .RethrowWhen((descriptor, context) => context.MessageId == "retry");
        backgroundBuilder
            .HandleWhen((exception, token) => exception is InvalidOperationException)
            .Rethrow()
            .Complete()
            .RethrowWhen(descriptor => descriptor.Kind == ExceptionKind.Failure)
            .Return(descriptor => descriptor.Code);

        Assert.True(consumerOptions.ShouldHandle(new Exception(), new ConsumerExceptionContext { DeliveryCount = 2 }, CancellationToken.None));
        Assert.True(consumerOptions.ShouldRethrow(new ExceptionDescriptor(), new ConsumerExceptionContext { MessageId = "retry" }));
        Assert.True(backgroundOptions.ShouldHandle(new InvalidOperationException(), CancellationToken.None));
        Assert.True(backgroundOptions.ShouldRethrow(new ExceptionDescriptor { Kind = ExceptionKind.Failure }));
        Assert.Equal("failure", backgroundOptions.DefaultResultFactory(new ExceptionDescriptor { Code = "failure" }));
    }

    [Fact]
    public async Task Exception_handlers_and_reporters_validate_arguments()
    {
        var handler = CreateHandler(_ => { });
        var backgroundReporter = new LoggingBackgroundExceptionReporter(NullLogger<LoggingBackgroundExceptionReporter>.Instance);
        var consumerReporter = new LoggingConsumerExceptionReporter(NullLogger<LoggingConsumerExceptionReporter>.Instance);

        Assert.Throws<ArgumentNullException>(() => handler.Handle(null));
        await Assert.ThrowsAsync<ArgumentNullException>(() => backgroundReporter.ReportAsync(null));
        await Assert.ThrowsAsync<ArgumentNullException>(() => consumerReporter.ReportAsync(null));
    }

    [Fact]
    public void Exception_handling_service_registration_extensions_validate_arguments()
    {
        IServiceCollection services = null;
        var validServices = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddTurtlePathExceptionHandlingCore());
        Assert.Throws<ArgumentNullException>(() => services.AddExceptionHandlingProfile<SampleExceptionHandlingProfile>());
        Assert.Throws<ArgumentNullException>(() => services.AddExceptionHandlingProfile(new SampleExceptionHandlingProfile()));
        Assert.Throws<ArgumentNullException>(() => validServices.AddExceptionHandlingProfile(null));
        Assert.Throws<ArgumentNullException>(() => services.AddExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly));
        Assert.Same(validServices, validServices.AddExceptionHandlingProfiles());
        Assert.Same(validServices, validServices.AddExceptionHandlingProfiles(null));
        Assert.Throws<ArgumentNullException>(() => services.AddTurtlePathAspNetCoreExceptionHandling());
        Assert.Throws<ArgumentNullException>(() => new HttpExceptionHandlingOptionsBuilder(new HttpExceptionHandlingOptions()).Map(null, StatusCodes.Status400BadRequest));
        Assert.Throws<ArgumentNullException>(() => services.AddHttpExceptionHandlingProfile<SampleHttpExceptionHandlingProfile>());
        Assert.Throws<ArgumentNullException>(() => services.AddHttpExceptionHandlingProfile(new SampleHttpExceptionHandlingProfile()));
        Assert.Throws<ArgumentNullException>(() => validServices.AddHttpExceptionHandlingProfile(null));
        Assert.Throws<ArgumentNullException>(() => services.AddHttpExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly));
        Assert.Same(validServices, validServices.AddHttpExceptionHandlingProfiles());
        Assert.Same(validServices, validServices.AddHttpExceptionHandlingProfiles(null));
        Assert.Throws<ArgumentNullException>(() => services.AddTurtlePathConsumerExceptionHandling());
        Assert.Throws<ArgumentNullException>(() => services.AddConsumerExceptionHandlingProfile<SampleConsumerExceptionHandlingProfile>());
        Assert.Throws<ArgumentNullException>(() => validServices.AddConsumerExceptionHandlingProfile(null));
        Assert.Throws<ArgumentNullException>(() => services.AddConsumerExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly));
        Assert.Same(validServices, validServices.AddConsumerExceptionHandlingProfiles());
        Assert.Same(validServices, validServices.AddConsumerExceptionHandlingProfiles(null));
        Assert.Throws<ArgumentNullException>(() => services.AddTurtlePathWorkerExceptionHandling());
        Assert.Throws<ArgumentNullException>(() => services.AddBackgroundExceptionHandlingProfile(new SampleBackgroundExceptionHandlingProfile()));
        Assert.Throws<ArgumentNullException>(() => services.AddBackgroundExceptionHandlingProfile<SampleBackgroundExceptionHandlingProfile>());
        Assert.Throws<ArgumentNullException>(() => validServices.AddBackgroundExceptionHandlingProfile(null));
        Assert.Throws<ArgumentNullException>(() => services.AddBackgroundExceptionHandlingProfiles(typeof(ExceptionHandlingTests).Assembly));
        Assert.Same(validServices, validServices.AddBackgroundExceptionHandlingProfiles());
        Assert.Same(validServices, validServices.AddBackgroundExceptionHandlingProfiles(null));

        validServices.AddExceptionHandlingProfile<SampleExceptionHandlingProfile>();
        validServices.AddHttpExceptionHandlingProfile<SampleHttpExceptionHandlingProfile>();
        validServices.AddConsumerExceptionHandlingProfile<SampleConsumerExceptionHandlingProfile>();
        validServices.AddConsumerExceptionHandlingProfile(new SampleConsumerExceptionHandlingProfile());
        validServices.AddBackgroundExceptionHandlingProfile<SampleBackgroundExceptionHandlingProfile>();
        validServices.AddBackgroundExceptionHandlingProfile(new SampleBackgroundExceptionHandlingProfile());
    }

    private static IExceptionHandler CreateHandler(Action<ExceptionHandlingOptionsBuilder> configure)
    {
        var options = new ExceptionHandlingOptions();
        configure(new ExceptionHandlingOptionsBuilder(options));

        return new DefaultExceptionHandler(Options.Create(options));
    }

    private sealed class SampleValidationException : Exception
    {
        public SampleValidationException(string field, params string[] errors)
            : base(errors.FirstOrDefault())
        {
            Field = field;
            Errors = errors;
        }

        public string Field { get; }

        public IReadOnlyCollection<string> Errors { get; }
    }

    private sealed class SampleMessage
    {
    }

    private static readonly ExceptionKind ProfileMappedExceptionKind = new("profile_mapped");

    private sealed class ProfileMappedException : Exception
    {
        public ProfileMappedException(string message)
            : base(message)
        {
        }
    }

    private sealed class SampleExceptionHandlingProfile : ExceptionHandlingProfile
    {
        public override void Configure(ExceptionHandlingOptionsBuilder builder)
        {
            builder.For<ProfileMappedException>(
                _ => ProfileMappedExceptionKind,
                _ => "profile_mapped",
                exception => [ exception.Message ]);
        }
    }

    private sealed class SampleHttpExceptionHandlingProfile : HttpExceptionHandlingProfile
    {
        public override void Configure(HttpExceptionHandlingOptionsBuilder builder)
        {
            builder.Map(ProfileMappedExceptionKind, StatusCodes.Status403Forbidden);
        }
    }

    private sealed class SampleConsumerExceptionHandlingProfile : ConsumerExceptionHandlingProfile
    {
        public override void Configure(ConsumerExceptionHandlingOptionsBuilder builder)
        {
            builder.RethrowWhen((descriptor, _) => descriptor.Kind != ProfileMappedExceptionKind);
        }
    }

    private sealed class SampleBackgroundExceptionHandlingProfile : BackgroundExceptionHandlingProfile
    {
        public override void Configure(BackgroundExceptionHandlingOptionsBuilder builder)
        {
            builder.RethrowWhen(descriptor => descriptor.Kind != ProfileMappedExceptionKind);
        }
    }
}
