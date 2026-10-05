using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Pelican.Mediator;
using Spider.Pipelines.Core;
using TurtlePath.Commands;
using TurtlePath.Commands.Steps;
using TurtlePath.Domain.Contracts;
using TurtlePath.Hooks;
using TurtlePath.Mapping;
using TurtlePath.Models.Requests;
using TurtlePath.Models.Responses;
using TurtlePath.Persistence;
using TurtlePath.Queries;
using TurtlePath.Validation;

namespace TurtlePath.Tests;

public class GenericEntityHandlerTests
{
    [Fact]
    public async Task Create_handler_supports_entities_with_custom_key_contract()
    {
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new CreateCustomEntityHandler(provider);

        var response = await handler.Handle(new CreateCustomEntityRequest("Ada"));

        var entity = Assert.Single(storage.AddedEntities.OfType<CustomEntity>());
        Assert.Equal(10, entity.Id);
        Assert.Equal("Ada", entity.Name);
        Assert.Equal(entity.Id, response.Id);
        Assert.Equal(entity.Name, response.Name);
    }

    [Fact]
    public async Task Create_handler_runs_command_hooks_by_stage_order()
    {
        var calls = new List<string>();
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<CreateCommandStageHook>();
            });

        var handler = new CreateCustomEntityHandler(provider);

        await handler.Handle(new CreateCustomEntityRequest("Ada"));

        Assert.Equal(
            [
                "before-validation",
                "after-validation",
                "before-map",
                "after-map",
                "before-save",
                "after-save",
                "before-response",
                "after-response"
            ],
            calls);
    }

    [Fact]
    public async Task Create_no_return_handler_supports_entities_with_custom_key_contract()
    {
        var calls = new List<string>();
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<CreateNoReturnCommandStageHook>();
            });

        var handler = new CreateCustomEntityNoReturnHandler(provider);

        await handler.Handle(new CreateCustomEntityCommand("Linus"));

        var entity = Assert.Single(storage.AddedEntities.OfType<CustomEntity>());
        Assert.Equal(11, entity.Id);
        Assert.Equal("Linus", entity.Name);
        Assert.Equal(
            [
                "before-validation",
                "after-validation",
                "before-map",
                "after-map",
                "before-save",
                "after-save"
            ],
            calls);
    }

    [Fact]
    public async Task Create_handler_uses_closed_creation_step_from_dependency_injection()
    {
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services => services.AddScoped<IEntityCreationStep<CreateCustomEntityRequest, CustomEntity>, ReplacingEntityCreationStep>());

        var handler = new CreateCustomEntityHandler(provider);

        var response = await handler.Handle(new CreateCustomEntityRequest("Ada"));

        Assert.Equal(77, response.Id);
        Assert.Equal("from-step", response.Name);
    }

    [Fact]
    public async Task Create_handler_uses_response_options_to_project_response_from_storage()
    {
        var storage = new RecordingStorageWriterAdapter();
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity
        {
            Id = 10,
            Name = "from-storage"
        });
        using var provider = CreateProvider(
            storage,
            reader,
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services => services.AddSingleton<ICommandResponseOptions<CreateCustomEntityRequest, CustomEntity>>(
                new ProjectedCreateResponseOptions()));

        var handler = new CreateCustomEntityHandler(provider);

        var response = await handler.Handle(new CreateCustomEntityRequest("Ada"));

        Assert.Equal(10, response.Id);
        Assert.Equal("from-storage", response.Name);
    }

    [Fact]
    public async Task Create_handler_virtual_override_takes_precedence_over_creation_step()
    {
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services => services.AddScoped<IEntityCreationStep<CreateCustomEntityRequest, CustomEntity>, ReplacingEntityCreationStep>());

        var handler = new OverrideCreateCustomEntityHandler(provider);

        var response = await handler.Handle(new CreateCustomEntityRequest("Ada"));

        Assert.Equal(88, response.Id);
        Assert.Equal("from-override", response.Name);
    }

    [Fact]
    public async Task Get_by_id_handler_supports_entities_with_custom_key_contract()
    {
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity
        {
            Id = 42,
            Name = "Grace"
        });

        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            reader,
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new GetCustomEntityByIdHandler(provider);

        var response = await handler.Handle(new GetCustomEntityByIdQuery(42));

        Assert.Equal(42, response.Id);
        Assert.Equal("Grace", response.Name);
    }

    [Fact]
    public async Task Get_by_id_handler_runs_query_hooks_by_stage_order()
    {
        var calls = new List<string>();
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity
        {
            Id = 42,
            Name = "Grace"
        });

        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            reader,
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services =>
            {
                services.AddSingleton(calls);
                services.AddHandlerHook<GetByIdQueryStageHook>();
            });

        var handler = new GetCustomEntityByIdHandler(provider);

        await handler.Handle(new GetCustomEntityByIdQuery(42));

        Assert.Equal(["before-query", "after-query"], calls);
    }

    [Fact]
    public async Task Update_handler_updates_loaded_entity_and_returns_response()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new UpdateCustomEntityHandler(provider);

        var response = await handler.Handle(new UpdateCustomEntityRequest { Id = 42, Name = "After" });

        Assert.Equal("After", entity.Name);
        Assert.Equal(42, entity.Id);
        Assert.Equal(1, storage.SaveChangesCount);
        Assert.Equal(42, response.Id);
        Assert.Equal("After", response.Name);
    }

    [Fact]
    public async Task Update_no_return_handler_updates_loaded_entity()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new UpdateCustomEntityNoReturnHandler(provider);

        await handler.Handle(new UpdateCustomEntityCommand { Id = 42, Name = "After" });

        Assert.Equal("After", entity.Name);
        Assert.Equal(42, entity.Id);
        Assert.Equal(1, storage.SaveChangesCount);
    }

    [Fact]
    public async Task Delete_handler_removes_loaded_entity_and_returns_response()
    {
        var entity = new CustomEntity { Id = 42, Name = "To delete" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new DeleteCustomEntityHandler(provider);

        var response = await handler.Handle(new DeleteCustomEntityRequest { Id = 42 });

        Assert.Same(entity, Assert.Single(storage.RemovedEntities));
        Assert.Equal(1, storage.SaveChangesCount);
        Assert.Equal(42, response.Id);
        Assert.Equal("To delete", response.Name);
    }

    [Fact]
    public async Task Delete_no_return_handler_removes_loaded_entity()
    {
        var entity = new CustomEntity { Id = 42, Name = "To delete" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new DeleteCustomEntityNoReturnHandler(provider);

        await handler.Handle(new DeleteCustomEntityCommand { Id = 42 });

        Assert.Same(entity, Assert.Single(storage.RemovedEntities));
        Assert.Equal(1, storage.SaveChangesCount);
    }

    [Fact]
    public async Task Patch_handler_applies_patch_and_returns_response()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new PatchCustomEntityHandler(provider);

        var response = await handler.Handle(new PatchCustomEntityRequest { Id = 42, Name = "Patched" });

        Assert.Equal("Patched", entity.Name);
        Assert.Equal(1, storage.SaveChangesCount);
        Assert.Equal(42, response.Id);
        Assert.Equal("Patched", response.Name);
    }

    [Fact]
    public async Task Patch_no_return_handler_applies_patch()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new PatchCustomEntityNoReturnHandler(provider);

        await handler.Handle(new PatchCustomEntityCommand { Id = 42, Name = "Patched" });

        Assert.Equal("Patched", entity.Name);
        Assert.Equal(1, storage.SaveChangesCount);
    }

    [Fact]
    public async Task Get_many_handler_returns_matching_responses()
    {
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new InMemoryStorageReaderAdapter(
                new CustomEntity { Id = 1, Name = "Ada" },
                new CustomEntity { Id = 2, Name = "Grace" }),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new GetManyCustomEntitiesHandler(provider);

        var responses = (await handler.Handle(new GetManyCustomEntitiesQuery())).ToArray();

        Assert.Equal([1, 2], responses.Select(response => response.Id));
        Assert.Equal(["Ada", "Grace"], responses.Select(response => response.Name));
    }

    [Fact]
    public async Task Get_one_handler_returns_matching_response_and_throws_when_missing()
    {
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new InMemoryStorageReaderAdapter(new CustomEntity { Id = 7, Name = "Seven" }),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new GetOneCustomEntityHandler(provider);

        var response = await handler.Handle(new GetOneCustomEntityQuery { Value = 7 });

        Assert.Equal(7, response.Id);
        Assert.Equal("Seven", response.Name);
        await Assert.ThrowsAsync<TurtlePath.Exceptions.NotFoundException>(() =>
            handler.Handle(new GetOneCustomEntityQuery { Value = 8 }));
    }

    [Fact]
    public async Task Get_one_handler_requires_options_for_non_key_value_type()
    {
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new InMemoryStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        var handler = new UnsupportedGetOneCustomEntityHandler(provider);

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            handler.Handle(new UnsupportedGetOneCustomEntityQuery { Value = "external" }));
    }

    [Fact]
    public async Task Command_handlers_execute_through_spider_when_registered()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services => services.AddSpider());

        var created = await new CreateCustomEntityHandler(provider).Handle(new CreateCustomEntityRequest("Created"));
        var updated = await new UpdateCustomEntityHandler(provider).Handle(new UpdateCustomEntityRequest { Id = 42, Name = "Updated" });
        var patched = await new PatchCustomEntityHandler(provider).Handle(new PatchCustomEntityRequest { Id = 42, Name = "Patched" });
        var deleted = await new DeleteCustomEntityHandler(provider).Handle(new DeleteCustomEntityRequest { Id = 42 });
        await new CreateCustomEntityNoReturnHandler(provider).Handle(new CreateCustomEntityCommand("NoReturn"));
        await new UpdateCustomEntityNoReturnHandler(provider).Handle(new UpdateCustomEntityCommand { Id = 42, Name = "NoReturnUpdated" });
        await new PatchCustomEntityNoReturnHandler(provider).Handle(new PatchCustomEntityCommand { Id = 42, Name = "NoReturnPatched" });
        await new DeleteCustomEntityNoReturnHandler(provider).Handle(new DeleteCustomEntityCommand { Id = 42 });

        Assert.Equal("Created", created.Name);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal("Patched", patched.Name);
        Assert.Equal("Patched", deleted.Name);
        Assert.Equal(2, storage.AddedEntities.Count);
        Assert.Equal(2, storage.RemovedEntities.Count);
    }

    [Fact]
    public async Task Command_handler_protected_dependencies_are_available_to_derived_handlers()
    {
        var storage = new RecordingStorageWriterAdapter();
        var reader = new InMemoryStorageReaderAdapter(new CustomEntity { Id = 42, Name = "Before" });
        var mapper = new TestMapperAdapter();
        var validator = new NoopValidatorAdapter();
        using var provider = CreateProvider(storage, reader, mapper, validator);

        Assert.All(new InspectableCreateCustomEntityHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableCreateCustomEntityNoReturnHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableUpdateCustomEntityHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableUpdateCustomEntityNoReturnHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableDeleteCustomEntityHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableDeleteCustomEntityNoReturnHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectablePatchCustomEntityHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectablePatchCustomEntityNoReturnHandler(provider).Inspect(), Assert.NotNull);
        Assert.All(new InspectableGetPagedCustomEntitiesHandler(provider).Inspect(), Assert.NotNull);
    }

    [Fact]
    public async Task Command_handlers_can_disable_validation()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        var storage = new RecordingStorageWriterAdapter();
        using var provider = CreateProvider(
            storage,
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new ThrowingValidatorAdapter());

        await new NoValidationCreateCustomEntityHandler(provider).Handle(new CreateCustomEntityRequest("Created"));
        await new NoValidationCreateCustomEntityNoReturnHandler(provider).Handle(new CreateCustomEntityCommand("Created"));
        await new NoValidationUpdateCustomEntityHandler(provider).Handle(new UpdateCustomEntityRequest { Id = 42, Name = "Updated" });
        await new NoValidationUpdateCustomEntityNoReturnHandler(provider).Handle(new UpdateCustomEntityCommand { Id = 42, Name = "Updated again" });
        await new NoValidationDeleteCustomEntityHandler(provider).Handle(new DeleteCustomEntityRequest { Id = 42 });
        await new NoValidationDeleteCustomEntityNoReturnHandler(provider).Handle(new DeleteCustomEntityCommand { Id = 42 });
        await new NoValidationPatchCustomEntityHandler(provider).Handle(new PatchCustomEntityRequest { Id = 42, Name = "Patched" });
        await new NoValidationPatchCustomEntityNoReturnHandler(provider).Handle(new PatchCustomEntityCommand { Id = 42, Name = "Patched again" });

        Assert.Equal("Patched again", entity.Name);
    }

    [Fact]
    public async Task Delete_and_patch_handlers_can_enable_validation()
    {
        var entity = new CustomEntity { Id = 42, Name = "Before" };
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new InMemoryStorageReaderAdapter(entity),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());

        await new ValidatingDeleteCustomEntityHandler(provider).Handle(new DeleteCustomEntityRequest { Id = 42 });
        await new ValidatingDeleteCustomEntityNoReturnHandler(provider).Handle(new DeleteCustomEntityCommand { Id = 42 });
        await new ValidatingPatchCustomEntityHandler(provider).Handle(new PatchCustomEntityRequest { Id = 42, Name = "Patched" });
        await new ValidatingPatchCustomEntityNoReturnHandler(provider).Handle(new PatchCustomEntityCommand { Id = 42, Name = "Patched again" });

        Assert.Equal("Patched again", entity.Name);
    }

    [Fact]
    public async Task Get_one_handler_uses_registered_query_options()
    {
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new InMemoryStorageReaderAdapter(new CustomEntity { Id = 42, Name = "Configured" }),
            new TestMapperAdapter(),
            new NoopValidatorAdapter(),
            services => services.AddSingleton<IGetOneQueryOptions<UnsupportedGetOneCustomEntityQuery, CustomEntity>, UnsupportedGetOneOptions>());

        var response = await new UnsupportedGetOneCustomEntityHandler(provider)
            .Handle(new UnsupportedGetOneCustomEntityQuery { Value = "42" });

        Assert.Equal("Configured", response.Name);
    }

    [Fact]
    public async Task Default_patch_step_requires_patch_action_request()
    {
        using var provider = CreateProvider(
            new RecordingStorageWriterAdapter(),
            new EmptyStorageReaderAdapter(),
            new TestMapperAdapter(),
            new NoopValidatorAdapter());
        var step = provider.GetRequiredService<IEntityPatchStep<UpdateCustomEntityRequest, CustomEntity>>();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await step.PatchAsync(new UpdateCustomEntityRequest(), new CustomEntity(), CancellationToken.None));

        Assert.Contains(nameof(IPatchAction<CustomEntity>), exception.Message);
    }

    private static ServiceProvider CreateProvider(
        IStorageWriterAdapter storageWriterAdapter,
        IStorageReaderAdapter storageReaderAdapter,
        IMapperAdapter mapperAdapter,
        IValidatorAdapter validatorAdapter,
        Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();

        services.AddTurtlePath();
        services.AddSingleton(storageWriterAdapter);
        services.AddSingleton(storageReaderAdapter);
        services.AddSingleton(mapperAdapter);
        services.AddSingleton(validatorAdapter);
        configure?.Invoke(services);

        return services.BuildServiceProvider();
    }

    private sealed record CreateCustomEntityRequest(string Name) : IRequest<CustomResponse>;

    private sealed record CreateCustomEntityCommand(string Name) : IRequest;

    private sealed class UpdateCustomEntityRequest : IBaseRequest<int>, IRequest<CustomResponse>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class UpdateCustomEntityCommand : IBaseRequest<int>, IRequest
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class DeleteCustomEntityRequest : IBaseRequest<int>, IRequest<CustomResponse>
    {
        public int Id { get; set; }
    }

    private sealed class DeleteCustomEntityCommand : IBaseRequest<int>, IRequest
    {
        public int Id { get; set; }
    }

    private sealed class PatchCustomEntityRequest : IBaseRequest<int>, IRequest<CustomResponse>, IPatchAction<CustomEntity>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ValueTask PatchAsync(CustomEntity entity, CancellationToken cancellationToken)
        {
            entity.Name = Name;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class PatchCustomEntityCommand : IBaseRequest<int>, IRequest, IPatchAction<CustomEntity>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ValueTask PatchAsync(CustomEntity entity, CancellationToken cancellationToken)
        {
            entity.Name = Name;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class GetManyCustomEntitiesQuery : GenericGetManyQuery<CustomEntity, CustomResponse, int>
    {
    }

    private sealed class GetPagedCustomEntitiesQuery : GenericGetPagedInfoQuery<CustomEntity, CustomResponse, int>
    {
        public GetPagedCustomEntitiesQuery() : base(new PagedSettings())
        {
        }
    }

    private sealed class GetOneCustomEntityQuery : GenericGetOneQuery<int, CustomEntity, CustomResponse, int>
    {
    }

    private sealed class UnsupportedGetOneCustomEntityQuery : GenericGetOneQuery<string, CustomEntity, CustomResponse, int>
    {
    }

    private sealed class GetCustomEntityByIdQuery : GenericGetByIdQuery<CustomEntity, CustomResponse, int>
    {
        public GetCustomEntityByIdQuery(int id) : base(id)
        {
        }
    }

    private sealed class CustomEntity : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class CustomResponse : IBaseResponse<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private sealed class CreateCustomEntityHandler
        : GenericCreateCommandHandler<CreateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public CreateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class OverrideCreateCustomEntityHandler
        : GenericCreateCommandHandler<CreateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public OverrideCreateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ValueTask<CustomEntity> MapToEntityAsync(CreateCustomEntityRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new CustomEntity
            {
                Id = 88,
                Name = "from-override"
            });
        }
    }

    private sealed class CreateCustomEntityNoReturnHandler
        : GenericCreateCommandHandler<CreateCustomEntityCommand, CustomEntity, int>
    {
        public CreateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableCreateCustomEntityHandler
        : GenericCreateCommandHandler<CreateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public InspectableCreateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, ValidationStep, EntityCreationStep, EntityAddStep, ResponseMappingStep];
    }

    private sealed class InspectableCreateCustomEntityNoReturnHandler
        : GenericCreateCommandHandler<CreateCustomEntityCommand, CustomEntity, int>
    {
        public InspectableCreateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, ValidatorAdapter, MapperAdapter, ValidationStep, EntityCreationStep, EntityAddStep];
    }

    private sealed class NoValidationCreateCustomEntityHandler
        : GenericCreateCommandHandler<CreateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public NoValidationCreateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class NoValidationCreateCustomEntityNoReturnHandler
        : GenericCreateCommandHandler<CreateCustomEntityCommand, CustomEntity, int>
    {
        public NoValidationCreateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class ReplacingEntityCreationStep : IEntityCreationStep<CreateCustomEntityRequest, CustomEntity>
    {
        public ValueTask<CustomEntity> CreateAsync(CreateCustomEntityRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new CustomEntity
            {
                Id = 77,
                Name = "from-step"
            });
        }
    }

    private sealed class ProjectedCreateResponseOptions : ICommandResponseOptions<CreateCustomEntityRequest, CustomEntity>
    {
        public bool UseProjectionFromStorage => true;

        public Expression<Func<CustomEntity, object>>[] GetIncludeExpressions(CreateCustomEntityRequest request)
            => [];
    }

    private sealed class GetCustomEntityByIdHandler
        : GenericGetByIdQueryHandler<GetCustomEntityByIdQuery, CustomEntity, CustomResponse, int>
    {
        public GetCustomEntityByIdHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class UpdateCustomEntityHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public UpdateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableUpdateCustomEntityHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public InspectableUpdateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityMappingStep, EntitySaveStep, ResponseMappingStep];
    }

    private sealed class NoValidationUpdateCustomEntityHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public NoValidationUpdateCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class UpdateCustomEntityNoReturnHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityCommand, CustomEntity, int>
    {
        public UpdateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableUpdateCustomEntityNoReturnHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityCommand, CustomEntity, int>
    {
        public InspectableUpdateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityMappingStep, EntitySaveStep];
    }

    private sealed class NoValidationUpdateCustomEntityNoReturnHandler
        : GenericUpdateCommandHandler<UpdateCustomEntityCommand, CustomEntity, int>
    {
        public NoValidationUpdateCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class DeleteCustomEntityHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public DeleteCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableDeleteCustomEntityHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public InspectableDeleteCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityDeleteStep];
    }

    private sealed class NoValidationDeleteCustomEntityHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public NoValidationDeleteCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class ValidatingDeleteCustomEntityHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public ValidatingDeleteCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => true;
    }

    private sealed class DeleteCustomEntityNoReturnHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityCommand, CustomEntity, int>
    {
        public DeleteCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableDeleteCustomEntityNoReturnHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityCommand, CustomEntity, int>
    {
        public InspectableDeleteCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityDeleteStep];
    }

    private sealed class NoValidationDeleteCustomEntityNoReturnHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityCommand, CustomEntity, int>
    {
        public NoValidationDeleteCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class ValidatingDeleteCustomEntityNoReturnHandler
        : GenericDeleteCommandHandler<DeleteCustomEntityCommand, CustomEntity, int>
    {
        public ValidatingDeleteCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => true;
    }

    private sealed class PatchCustomEntityHandler
        : GenericPatchCommandHandler<PatchCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public PatchCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectablePatchCustomEntityHandler
        : GenericPatchCommandHandler<PatchCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public InspectablePatchCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityPatchStep, EntitySaveStep, ResponseMappingStep];
    }

    private sealed class NoValidationPatchCustomEntityHandler
        : GenericPatchCommandHandler<PatchCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public NoValidationPatchCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class ValidatingPatchCustomEntityHandler
        : GenericPatchCommandHandler<PatchCustomEntityRequest, CustomResponse, CustomEntity, int>
    {
        public ValidatingPatchCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => true;
    }

    private sealed class PatchCustomEntityNoReturnHandler
        : GenericPatchCommandHandler<PatchCustomEntityCommand, CustomEntity, int>
    {
        public PatchCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectablePatchCustomEntityNoReturnHandler
        : GenericPatchCommandHandler<PatchCustomEntityCommand, CustomEntity, int>
    {
        public InspectablePatchCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [Services, StorageWriterAdapter, StorageReaderAdapter, ValidatorAdapter, MapperAdapter, EntityLookupStep, ValidationStep, EntityPatchStep, EntitySaveStep];
    }

    private sealed class NoValidationPatchCustomEntityNoReturnHandler
        : GenericPatchCommandHandler<PatchCustomEntityCommand, CustomEntity, int>
    {
        public NoValidationPatchCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => false;
    }

    private sealed class ValidatingPatchCustomEntityNoReturnHandler
        : GenericPatchCommandHandler<PatchCustomEntityCommand, CustomEntity, int>
    {
        public ValidatingPatchCustomEntityNoReturnHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override bool ValidateRequest => true;
    }

    private sealed class GetManyCustomEntitiesHandler
        : GenericGetManyQueryHandler<GetManyCustomEntitiesQuery, CustomEntity, CustomResponse, int>
    {
        public GetManyCustomEntitiesHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class GetOneCustomEntityHandler
        : GenericGetOneQueryHandler<GetOneCustomEntityQuery, int, CustomEntity, CustomResponse, int>
    {
        public GetOneCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class InspectableGetPagedCustomEntitiesHandler
        : GenericGetPagedInfoQueryHandler<GetPagedCustomEntitiesQuery, CustomEntity, CustomResponse, int>
    {
        public InspectableGetPagedCustomEntitiesHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public object[] Inspect()
            => [ServiceProvider, StorageReaderAdapter, DefaultPageSize, DefaultPageNumber];
    }

    private sealed class UnsupportedGetOneCustomEntityHandler
        : GenericGetOneQueryHandler<UnsupportedGetOneCustomEntityQuery, string, CustomEntity, CustomResponse, int>
    {
        public UnsupportedGetOneCustomEntityHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    private sealed class UnsupportedGetOneOptions : IGetOneQueryOptions<UnsupportedGetOneCustomEntityQuery, CustomEntity>
    {
        public Expression<Func<CustomEntity, bool>> GetFilterExpression(UnsupportedGetOneCustomEntityQuery query)
            => entity => entity.Id == int.Parse(query.Value);

        public Expression<Func<CustomEntity, object>>[] GetIncludeExpressions(UnsupportedGetOneCustomEntityQuery query)
            => [];

        public string NotFoundMessage => null;
    }

    private sealed class CreateCommandStageHook(List<string> calls) :
        IBeforeValidationHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterValidationHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeMapHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterMapHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeSaveHook<CreateCustomEntityRequest, CustomEntity>,
        IAfterSaveHook<CreateCustomEntityRequest, CustomEntity>,
        IBeforeResponseHook<CreateCustomEntityRequest, CustomEntity, CustomResponse>,
        IAfterResponseHook<CreateCustomEntityRequest, CustomEntity, CustomResponse>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-validation");

        public ValueTask AfterValidationAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-validation");

        public ValueTask BeforeMapAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-map");

        public ValueTask AfterMapAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-map");

        public ValueTask BeforeSaveAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-save");

        public ValueTask AfterSaveAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-save");

        public ValueTask BeforeResponseAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("before-response");

        public ValueTask AfterResponseAsync(CommandHookContext<CreateCustomEntityRequest, CustomEntity, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("after-response");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CreateNoReturnCommandStageHook(List<string> calls) :
        IBeforeValidationHook<CreateCustomEntityCommand, CustomEntity>,
        IAfterValidationHook<CreateCustomEntityCommand, CustomEntity>,
        IBeforeMapHook<CreateCustomEntityCommand, CustomEntity>,
        IAfterMapHook<CreateCustomEntityCommand, CustomEntity>,
        IBeforeSaveHook<CreateCustomEntityCommand, CustomEntity>,
        IAfterSaveHook<CreateCustomEntityCommand, CustomEntity>
    {
        public ValueTask BeforeValidationAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-validation");

        public ValueTask AfterValidationAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-validation");

        public ValueTask BeforeMapAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-map");

        public ValueTask AfterMapAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-map");

        public ValueTask BeforeSaveAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("before-save");

        public ValueTask AfterSaveAsync(CommandHookContext<CreateCustomEntityCommand, CustomEntity> context, CancellationToken cancellationToken = default)
            => AddAsync("after-save");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class GetByIdQueryStageHook(List<string> calls) :
        IBeforeQueryHook<GetCustomEntityByIdQuery, CustomResponse>,
        IAfterQueryHook<GetCustomEntityByIdQuery, CustomResponse>
    {
        public ValueTask BeforeQueryAsync(QueryHookContext<GetCustomEntityByIdQuery, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("before-query");

        public ValueTask AfterQueryAsync(QueryHookContext<GetCustomEntityByIdQuery, CustomResponse> context, CancellationToken cancellationToken = default)
            => AddAsync("after-query");

        private ValueTask AddAsync(string call)
        {
            calls.Add(call);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestMapperAdapter : IMapperAdapter
    {
        public ValueTask<TDestination> MapAsync<TSource, TDestination>(
            TSource source,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            object result = source switch
            {
                CreateCustomEntityRequest request when typeof(TDestination) == typeof(CustomEntity) => new CustomEntity
                {
                    Id = 10,
                    Name = request.Name
                },
                CreateCustomEntityCommand request when typeof(TDestination) == typeof(CustomEntity) => new CustomEntity
                {
                    Id = 11,
                    Name = request.Name
                },
                CustomEntity entity when typeof(TDestination) == typeof(CustomResponse) => new CustomResponse
                {
                    Id = entity.Id,
                    Name = entity.Name
                },
                _ => throw new InvalidOperationException($"Mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not configured.")
            };

            return ValueTask.FromResult((TDestination)result);
        }

        public ValueTask UpdateMapAsync<TSource, TDestination>(
            TSource source,
            TDestination destination,
            CancellationToken cancellationToken = default)
            where TSource : class
            where TDestination : class
        {
            switch (source, destination)
            {
                case (UpdateCustomEntityRequest request, CustomEntity entity):
                    entity.Name = request.Name;
                    break;
                case (UpdateCustomEntityCommand request, CustomEntity entity):
                    entity.Name = request.Name;
                    break;
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class NoopValidatorAdapter : IValidatorAdapter
    {
        public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    private sealed class ThrowingValidatorAdapter : IValidatorAdapter
    {
        public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Validation should be disabled for this test.");
    }

    private sealed class RecordingStorageWriterAdapter : IStorageWriterAdapter
    {
        public List<object> AddedEntities { get; } = [];

        public List<object> RemovedEntities { get; } = [];

        public int SaveChangesCount { get; private set; }

        public ValueTask AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            AddedEntities.Add(entity);
            return ValueTask.CompletedTask;
        }

        public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            AddedEntities.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
        }

        public void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity
        {
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class, IEntity
        {
            RemovedEntities.Add(entity);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        public Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => Task.CompletedTask;

        public Task UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            => Task.CompletedTask;

        public Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
        {
            RemovedEntities.Add(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyStorageReaderAdapter : IStorageReaderAdapter
    {
        public IStorageReadSet<TEntity> For<TEntity>() where TEntity : class, IEntity
            => new InMemoryStorageReadSet<TEntity>([]);

        public Task<TExpected> GetOneAsync<TEntity, TExpected>(
            GetOneCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => Task.FromResult<TExpected>(null);

        public Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(
            GetManyCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => Task.FromResult(new BatchResult<TExpected>());
    }

    private sealed class InMemoryStorageReaderAdapter : IStorageReaderAdapter
    {
        private readonly IReadOnlyCollection<object> entities;

        public InMemoryStorageReaderAdapter(params object[] entities)
        {
            this.entities = entities;
        }

        public IStorageReadSet<TEntity> For<TEntity>() where TEntity : class, IEntity
            => new InMemoryStorageReadSet<TEntity>(entities.OfType<TEntity>().ToList());

        public Task<TExpected> GetOneAsync<TEntity, TExpected>(
            GetOneCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => For<TEntity>()
                .Where(criteria.FiltersExpression)
                .FirstOrDefaultAsync<TExpected>(cancellationToken);

        public Task<BatchResult<TExpected>> GetManyAsync<TEntity, TExpected>(
            GetManyCriteria<TEntity> criteria,
            CancellationToken cancellationToken = default)
            where TEntity : class, IEntity
            where TExpected : class
            => For<TEntity>().ToBatchAsync<TExpected>(cancellationToken);
    }

    private sealed class InMemoryStorageReadSet<TEntity> : IStorageReadSet<TEntity>
        where TEntity : class, IEntity
    {
        private IEnumerable<TEntity> entities;

        public InMemoryStorageReadSet(IEnumerable<TEntity> entities)
        {
            this.entities = entities;
        }

        public IStorageReadSet<TEntity> Where(Expression<Func<TEntity, bool>> filter)
        {
            if (filter != null)
                entities = entities.Where(filter.Compile()).ToList();

            return this;
        }

        public IStorageReadSet<TEntity> FilterBy(string filters) => this;

        public IStorageReadSet<TEntity> Include(params Expression<Func<TEntity, object>>[] includes) => this;

        public IStorageReadSet<TEntity> SortBy(Expression<Func<TEntity, object>> sort) => this;

        public IStorageReadSet<TEntity> SortByDescending(Expression<Func<TEntity, object>> sort) => this;

        public IStorageReadSet<TEntity> SortBy(string sorts) => this;

        public IStorageReadSet<TEntity> AsTracking() => this;

        public IStorageReadSet<TEntity> AsNoTracking() => this;

        public IStorageReadSet<TEntity> Page(int? pageNumber, int? pageSize) => this;

        public Task<TExpected> FirstOrDefaultAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var entity = entities.FirstOrDefault();

            return Task.FromResult(Map<TExpected>(entity));
        }

        public Task<BatchResult<TExpected>> ToBatchAsync<TExpected>(CancellationToken cancellationToken = default)
            where TExpected : class
        {
            var results = entities
                .Select(Map<TExpected>)
                .Where(result => result != null)
                .ToArray();

            return Task.FromResult(new BatchResult<TExpected>
            {
                Results = results
            });
        }

        private static TExpected Map<TExpected>(TEntity entity)
            where TExpected : class
        {
            if (entity == null)
                return null;

            if (entity is TExpected expected)
                return expected;

            if (entity is CustomEntity customEntity && typeof(TExpected) == typeof(CustomResponse))
            {
                return new CustomResponse
                {
                    Id = customEntity.Id,
                    Name = customEntity.Name
                } as TExpected;
            }

            throw new InvalidOperationException($"Projection from {typeof(TEntity).Name} to {typeof(TExpected).Name} is not configured.");
        }
    }
}
