namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using Spider.Pipelines.Core;
    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using TurtlePath.Commands.Steps;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides a base implementation for handling update commands that return a response, including entity retrieval, validation, mapping, updating, and response mapping.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being updated.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericUpdateCommandHandler<TRequest, TResponse, TEntity, TKey> : BaseCommandHandler<TRequest, TResponse>
        where TRequest : class, IBaseRequest<TKey>, IRequest<TResponse>
        where TResponse : class, IBaseResponse<TKey>
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Gets the Spider pipeline instance used to describe and trace the handler flow when available.
        /// </summary>
        protected ISpider Spider { get; }

        /// <summary>
        /// Gets the storage adapter for saving and updating entities.
        /// </summary>
        protected IStorageWriterAdapter StorageWriterAdapter { get; }

        /// <summary>
        /// Gets the storage adapter for reading entities.
        /// </summary>
        protected IStorageReaderAdapter StorageReaderAdapter { get; }

        /// <summary>
        /// Gets the validator adapter for validating requests.
        /// </summary>
        protected IValidatorAdapter ValidatorAdapter { get; }

        /// <summary>
        /// Gets the mapper adapter for mapping between types.
        /// </summary>
        protected IMapperAdapter MapperAdapter { get; }

        /// <summary>
        /// Gets the entity lookup step.
        /// </summary>
        protected IEntityLookupStep<TRequest, TEntity, TKey> EntityLookupStep { get; }

        /// <summary>
        /// Gets the request validation step.
        /// </summary>
        protected IRequestValidationStep<TRequest, TEntity> ValidationStep { get; }

        /// <summary>
        /// Gets the entity mapping step.
        /// </summary>
        protected IEntityMappingStep<TRequest, TEntity> EntityMappingStep { get; }

        /// <summary>
        /// Gets the entity save step.
        /// </summary>
        protected IEntitySaveStep<TRequest, TEntity> EntitySaveStep { get; }

        /// <summary>
        /// Gets the response mapping step.
        /// </summary>
        protected IResponseMappingStep<TRequest, TEntity, TResponse, TKey> ResponseMappingStep { get; }

        /// <summary>
        /// Gets optional response mapping options for this request/entity pair.
        /// </summary>
        protected ICommandResponseOptions<TRequest, TEntity> ResponseOptions { get; }

        /// <summary>
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => true;

        private readonly ICommandHookStageRunner<TRequest, TEntity, TResponse> hookStageRunner;

        /// <summary>
        /// Gets a value indicating whether to use a projection from storage for the response mapping.
        /// </summary>
        protected virtual bool UseProjectionFromStorage => ResponseOptions?.UseProjectionFromStorage ?? false;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity, TResponse> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericUpdateCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityMappingStep = Services.GetRequiredService<IEntityMappingStep<TRequest, TEntity>>();
            EntitySaveStep = Services.GetRequiredService<IEntitySaveStep<TRequest, TEntity>>();
            ResponseMappingStep = Services.GetRequiredService<IResponseMappingStep<TRequest, TEntity, TResponse, TKey>>();
            ResponseOptions = Services.GetService<ICommandResponseOptions<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity, TResponse>>();
        }

        /// <summary>
        /// Handles the update command by retrieving, validating, mapping, updating the entity, and returning the response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response for the update command as the result.</returns>
        public override async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity, TResponse>(request);

            if (Spider != null)
            {
                return await Spider
                    .ComposeFlow<TRequest, TResponse>("Update command")
                    .Describe("Loads an existing entity, validates the update request, maps changes, saves the entity, and maps the response.")
                    .Tags("turtlepath", "command", "update")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(LoadUpdateEntityAsync, step => step
                        .Named("Load entity")
                        .Describe("Runs before/after get entity hooks and loads the entity targeted by the request.")
                        .Tags("lookup", "hooks"))
                    .Then(ValidateUpdateRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the request against the loaded entity.")
                        .Tags("validation", "hooks"))
                    .Then(MapUpdateEntityAsync, step => step
                        .Named("Map entity")
                        .Describe("Runs before/after map hooks and applies request values to the loaded entity.")
                        .Tags("mapping", "hooks"))
                    .Then(SaveUpdatedEntityAsync, step => step
                        .Named("Save entity")
                        .Describe("Runs before/after save hooks and persists the updated entity.")
                        .Tags("persistence", "hooks"))
                    .Then(BuildUpdateResponseAsync, step => step
                        .Named("Map response")
                        .Describe("Runs before/after response hooks and maps the updated entity to the command response.")
                        .Tags("response", "projection", "hooks"))
                    .RunAsync(request, cancellationToken);
            }

            var entity = await LoadUpdateEntityAsync(request, cancellationToken);
            await ValidateUpdateRequestAsync(entity, cancellationToken);
            await MapUpdateEntityAsync(entity, cancellationToken);
            await SaveUpdatedEntityAsync(entity, cancellationToken);
            return await BuildUpdateResponseAsync(entity, cancellationToken);
        }

        private async Task<TEntity> LoadUpdateEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> ValidateUpdateRequestAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(Context.Request, entity, cancellationToken);

            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> MapUpdateEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            await MapEntityAsync(Context.Request, entity, cancellationToken);

            await hookStageRunner.AfterMapAsync(Context, cancellationToken);
            return entity;
        }

        private async Task SaveUpdatedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await UpdateEntityAsync(Context.Request, entity, cancellationToken);

            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);
        }

        private async Task<TResponse> BuildUpdateResponseAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeResponseAsync(Context, cancellationToken);
            var response = await MapToResponseAsync(Context.Request, entity, cancellationToken);
            Context.Response = response;

            await hookStageRunner.AfterResponseAsync(Context, cancellationToken);

            return response;
        }

        /// <summary>
        /// Retrieves the entity to be updated based on the request. Throws <see cref="NotFoundException"/> if the entity is not found.
        /// </summary>
        /// <param name="request">The request containing information to identify the entity.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the entity as the result.</returns>
        /// <exception cref="NotFoundException">Thrown if the entity is not found.</exception>
        protected virtual async Task<TEntity> GetEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            return await EntityLookupStep.GetAsync(request, request.Id, cancellationToken);
        }

        /// <summary>
        /// Validates the request and entity using the validator adapter if <see cref="ValidateRequest"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="entity">The entity to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous validation operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Maps the request onto the entity using the mapper adapter.
        /// </summary>
        /// <param name="request">The request containing updated values.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation.</returns>
        protected virtual async ValueTask MapEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            var entityId = entity.Id;

            await EntityMappingStep.MapAsync(request, entity, cancellationToken);

            entity.Id = entityId;
        }

        /// <summary>
        /// Updates the entity in the storage using the storage adapter.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        protected virtual Task UpdateEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => EntitySaveStep.SaveAsync(request, entity, cancellationToken);

        /// <summary>
        /// Maps the updated entity to a response using the mapper adapter or retrieves a projection from storage if <see cref="UseProjectionFromStorage"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The updated entity to map to a response.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped response as the result.</returns>
        protected virtual async ValueTask<TResponse> MapToResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => await ResponseMappingStep.MapAsync(
                request,
                entity,
                UseProjectionFromStorage,
                EntityKeyExpression.Equals<TEntity, TKey>(request.Id),
                GetResponseIncludeExpressions(request),
                cancellationToken);

        /// <summary>
        /// Gets navigation expressions to include when the response is projected from storage.
        /// </summary>
        /// <param name="request">The request being handled.</param>
        /// <returns>The navigation expressions to include.</returns>
        protected virtual Expression<Func<TEntity, object>>[] GetResponseIncludeExpressions(TRequest request)
            => ResponseOptions?.GetIncludeExpressions(request) ?? [];
    }

    /// <summary>
    /// Provides a base implementation for update commands that do not return a response.
    /// </summary>
    public abstract class GenericUpdateCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
        where TRequest : class, IBaseRequest<TKey>, IRequest
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Gets the Spider pipeline instance used to describe and trace the handler flow when available.
        /// </summary>
        protected ISpider Spider { get; }

        /// <summary>
        /// Gets the storage adapter for saving entities.
        /// </summary>
        protected IStorageWriterAdapter StorageWriterAdapter { get; }

        /// <summary>
        /// Gets the storage adapter for reading entities.
        /// </summary>
        protected IStorageReaderAdapter StorageReaderAdapter { get; }

        /// <summary>
        /// Gets the validator adapter for validating requests.
        /// </summary>
        protected IValidatorAdapter ValidatorAdapter { get; }

        /// <summary>
        /// Gets the mapper adapter for mapping between types.
        /// </summary>
        protected IMapperAdapter MapperAdapter { get; }

        /// <summary>
        /// Gets the entity lookup step.
        /// </summary>
        protected IEntityLookupStep<TRequest, TEntity, TKey> EntityLookupStep { get; }

        /// <summary>
        /// Gets the request validation step.
        /// </summary>
        protected IRequestValidationStep<TRequest, TEntity> ValidationStep { get; }

        /// <summary>
        /// Gets the entity mapping step.
        /// </summary>
        protected IEntityMappingStep<TRequest, TEntity> EntityMappingStep { get; }

        /// <summary>
        /// Gets the entity save step.
        /// </summary>
        protected IEntitySaveStep<TRequest, TEntity> EntitySaveStep { get; }

        /// <summary>
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => true;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity> Context { get; private set; }

        private readonly ICommandHookStageRunner<TRequest, TEntity> hookStageRunner;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericUpdateCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityMappingStep = Services.GetRequiredService<IEntityMappingStep<TRequest, TEntity>>();
            EntitySaveStep = Services.GetRequiredService<IEntitySaveStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity>>();
        }

        /// <summary>
        /// Handles the update command by retrieving, validating, mapping, and saving the entity.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity>(request);

            if (Spider != null)
            {
                await Spider
                    .ComposeFlow<TRequest>("Update command")
                    .Describe("Loads an existing entity, validates the update request, maps changes, and saves the entity.")
                    .Tags("turtlepath", "command", "update")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(LoadUpdateEntityAsync, step => step
                        .Named("Load entity")
                        .Describe("Runs before/after get entity hooks and loads the entity targeted by the request.")
                        .Tags("lookup", "hooks"))
                    .Then(ValidateUpdateRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the request against the loaded entity.")
                        .Tags("validation", "hooks"))
                    .Then(MapUpdateEntityAsync, step => step
                        .Named("Map entity")
                        .Describe("Runs before/after map hooks and applies request values to the loaded entity.")
                        .Tags("mapping", "hooks"))
                    .Then(SaveUpdatedEntityAsync, step => step
                        .Named("Save entity")
                        .Describe("Runs before/after save hooks and persists the updated entity.")
                        .Tags("persistence", "hooks"))
                    .RunAsync(request, cancellationToken);

                return;
            }

            var entity = await LoadUpdateEntityAsync(request, cancellationToken);
            await ValidateUpdateRequestAsync(entity, cancellationToken);
            await MapUpdateEntityAsync(entity, cancellationToken);
            await SaveUpdatedEntityAsync(entity, cancellationToken);
        }

        private async Task<TEntity> LoadUpdateEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;
            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> ValidateUpdateRequestAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(Context.Request, entity, cancellationToken);
            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> MapUpdateEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            await MapEntityAsync(Context.Request, entity, cancellationToken);
            await hookStageRunner.AfterMapAsync(Context, cancellationToken);
            return entity;
        }

        private async Task SaveUpdatedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await UpdateEntityAsync(Context.Request, entity, cancellationToken);
            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);
        }

        /// <summary>
        /// Retrieves the entity to update.
        /// </summary>
        /// <param name="request">The request containing the entity identifier.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The entity to update.</returns>
        protected virtual async Task<TEntity> GetEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            return await EntityLookupStep.GetAsync(request, request.Id, cancellationToken);
        }

        /// <summary>
        /// Validates the request using the validator adapter when validation is enabled.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="entity">The entity being updated.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, entity, cancellationToken);
        }

        /// <summary>
        /// Maps the request onto the entity.
        /// </summary>
        /// <param name="request">The request to map.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async ValueTask MapEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            var entityId = entity.Id;

            await EntityMappingStep.MapAsync(request, entity, cancellationToken);

            entity.Id = entityId;
        }

        /// <summary>
        /// Saves the updated entity.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task UpdateEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => EntitySaveStep.SaveAsync(request, entity, cancellationToken);
    }
}
