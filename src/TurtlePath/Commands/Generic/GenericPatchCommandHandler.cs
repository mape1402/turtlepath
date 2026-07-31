namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
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
    /// Provides a base implementation for handling patch commands that return a response, including entity retrieval, validation, patching, updating, and response mapping.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being patched.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericPatchCommandHandler<TRequest, TResponse, TEntity, TKey> : BaseCommandHandler<TRequest, TResponse>
        where TRequest : class, IBaseRequest<TKey>, IRequest<TResponse>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

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
        /// Gets the entity save step.
        /// </summary>
        protected IEntitySaveStep<TRequest, TEntity> EntitySaveStep { get; }

        /// <summary>
        /// Gets the entity patch step.
        /// </summary>
        protected IEntityPatchStep<TRequest, TEntity> EntityPatchStep { get; }

        /// <summary>
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => false;

        private readonly ICommandHookStageRunner<TRequest, TEntity, TResponse> hookStageRunner;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity, TResponse> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericPatchCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntitySaveStep = Services.GetRequiredService<IEntitySaveStep<TRequest, TEntity>>();
            EntityPatchStep = Services.GetRequiredService<IEntityPatchStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity, TResponse>>();
        }

        /// <summary>
        /// Handles the patch command by retrieving, validating, patching, updating the entity, and returning the response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response for the patch command as the result.</returns>
        public override async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity, TResponse>(request);

            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);

            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);

            await hookStageRunner.BeforePatchAsync(Context, cancellationToken);
            await PatchEntityAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterPatchAsync(Context, cancellationToken);

            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await UpdateEntityAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);

            await hookStageRunner.BeforeResponseAsync(Context, cancellationToken);
            var response = await BuildResponseAsync(request, entity, cancellationToken);
            Context.Response = response;

            await hookStageRunner.AfterResponseAsync(Context, cancellationToken);

            return response;
        }

        /// <summary>
        /// Retrieves the entity to be patched based on the request. Throws <see cref="NotFoundException"/> if the entity is not found.
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

            return ValidationStep.ValidateAsync(request, entity, cancellationToken);
        }

        /// <summary>
        /// Applies the patch from the request onto the entity.
        /// </summary>
        /// <param name="request">The request containing patch data.</param>
        /// <param name="entity">The entity to patch.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous patch operation.</returns>
        protected virtual ValueTask PatchEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => EntityPatchStep.PatchAsync(request, entity, cancellationToken);

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
        /// Maps the updated entity to a response using the mapper adapter. Must be implemented by derived classes.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The updated entity to map to a response.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped response as the result.</returns>
        protected abstract ValueTask<TResponse> BuildResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides a base implementation for patch commands that do not return a response.
    /// </summary>
    public abstract class GenericPatchCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
        where TRequest : class, IBaseRequest<TKey>, IRequest
        where TEntity : class, IEntity<TKey>
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

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
        /// Gets the entity save step.
        /// </summary>
        protected IEntitySaveStep<TRequest, TEntity> EntitySaveStep { get; }

        /// <summary>
        /// Gets the entity patch step.
        /// </summary>
        protected IEntityPatchStep<TRequest, TEntity> EntityPatchStep { get; }

        /// <summary>
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => false;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity> Context { get; private set; }

        private readonly ICommandHookStageRunner<TRequest, TEntity> hookStageRunner;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GenericPatchCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntitySaveStep = Services.GetRequiredService<IEntitySaveStep<TRequest, TEntity>>();
            EntityPatchStep = Services.GetRequiredService<IEntityPatchStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity>>();
        }

        /// <summary>
        /// Handles the patch command by retrieving, validating, patching, and saving the entity.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity>(request);

            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;
            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);

            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, entity, cancellationToken);
            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);

            await hookStageRunner.BeforePatchAsync(Context, cancellationToken);
            await PatchEntityAsync(request, entity, cancellationToken);
            await hookStageRunner.AfterPatchAsync(Context, cancellationToken);

            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await UpdateEntityAsync(request, entity, cancellationToken);
            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);
        }

        /// <summary>
        /// Retrieves the entity to patch.
        /// </summary>
        /// <param name="request">The request containing the entity identifier.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The entity to patch.</returns>
        protected virtual async Task<TEntity> GetEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            return await EntityLookupStep.GetAsync(request, request.Id, cancellationToken);
        }

        /// <summary>
        /// Validates the request using the validator adapter when validation is enabled.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="entity">The entity being patched.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, entity, cancellationToken);
        }

        /// <summary>
        /// Applies request changes to the entity.
        /// </summary>
        /// <param name="request">The request containing patch data.</param>
        /// <param name="entity">The entity to patch.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask PatchEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => EntityPatchStep.PatchAsync(request, entity, cancellationToken);

        /// <summary>
        /// Saves the patched entity.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task UpdateEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => EntitySaveStep.SaveAsync(request, entity, cancellationToken);
    }
}
