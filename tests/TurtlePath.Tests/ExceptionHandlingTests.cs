using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
