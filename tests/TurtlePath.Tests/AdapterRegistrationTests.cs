using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TurtlePath.Mapping;
using TurtlePath.Validation;
using TurtlePath.AutoMapper;
using TurtlePath.FluentValidation;

namespace TurtlePath.Tests;

public class AdapterRegistrationTests
{
    [Fact]
    public async Task UseAutoMapper_registers_mapper_adapter()
    {
        var services = new ServiceCollection();
        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>();
        }, NullLoggerFactory.Instance);

        services.AddSingleton(mapperConfiguration.CreateMapper());
        services
            .AddTurtlePath()
            .UseAutoMapper();

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IMapperAdapter>();

        var result = await adapter.MapAsync<SourceModel, DestinationModel>(new SourceModel { Name = "Ada" });

        Assert.IsType<TurtlePath.AutoMapper.MapperAdapter>(adapter);
        Assert.Equal("Ada", result.Name);
    }

    [Fact]
    public async Task AutoMapper_adapter_updates_existing_destination()
    {
        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.CreateMap<SourceModel, DestinationModel>();
        }, NullLoggerFactory.Instance);

        var adapter = new TurtlePath.AutoMapper.MapperAdapter(mapperConfiguration.CreateMapper());
        var destination = new DestinationModel();

        await adapter.UpdateMapAsync(new SourceModel { Name = "Grace" }, destination);

        Assert.Equal("Grace", destination.Name);
    }

    [Fact]
    public async Task UseFluentValidation_registers_validator_adapter()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IValidator<ValidationModel>, ValidationModelValidator>();
        services
            .AddTurtlePath()
            .UseFluentValidation();

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IValidatorAdapter>();

        await adapter.ValidateAsync(new ValidationModel { Name = "Ada" });

        Assert.IsType<TurtlePath.FluentValidation.ValidatorAdapter>(adapter);
    }

    [Fact]
    public async Task FluentValidation_adapter_throws_turtlepath_validation_exception()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IValidator<ValidationModel>, ValidationModelValidator>();
        services
            .AddTurtlePath()
            .UseFluentValidation();

        using var provider = services.BuildServiceProvider();
        var adapter = provider.GetRequiredService<IValidatorAdapter>();

        var exception = await Assert.ThrowsAsync<TurtlePath.Validation.ValidationException>(
            async () => await adapter.ValidateAsync(new ValidationModel()));

        Assert.Contains("Name:", exception.Errors.Single());
    }

    private sealed class SourceModel
    {
        public string Name { get; set; }
    }

    private sealed class DestinationModel
    {
        public string Name { get; set; }
    }

    private sealed class ValidationModel
    {
        public string Name { get; set; }
    }

    private sealed class ValidationModelValidator : AbstractValidator<ValidationModel>
    {
        public ValidationModelValidator()
        {
            RuleFor(model => model.Name).NotEmpty();
        }
    }
}
