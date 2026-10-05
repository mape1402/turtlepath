namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using Spider.Pipelines.Core;
    using TurtlePath.Commands.Steps;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Requests;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides a base implementation for handling delete commands that return a response, including entity retrieval, validation, deletion, and response building.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being deleted.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericDeleteCommandHandler<TRequest, TResponse, TEntity, TKey> : BaseCommandHandler<TRequest, TResponse>
        where TRequest : class, IBaseRequest<TKey>, IRequest<TResponse>
        where TResponse : class
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
        /// Gets the storage adapter for deleting entities.
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
        /// Gets the entity delete step.
        /// </summary>
        protected IEntityDeleteStep<TRequest, TEntity> EntityDeleteStep { get; }

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
        protected GenericDeleteCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityDeleteStep = Services.GetRequiredService<IEntityDeleteStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity, TResponse>>();
        }

        /// <summary>
        /// Handles the delete command by retrieving, validating, deleting the entity, and building the response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response for the delete command as the result.</returns>
        public override async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity, TResponse>(request);

            if (Spider != null)
            {
                return await Spider
                    .ComposeFlow<TRequest, TResponse>("Delete command")
                    .Describe("Loads an existing entity, optionally validates the delete request, deletes the entity, and maps a response.")
                    .Tags("turtlepath", "command", "delete")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(LoadDeleteEntityAsync, step => step
                        .Named("Load entity")
                        .Describe("Runs before/after get entity hooks and loads the entity targeted by the delete request.")
                        .Tags("lookup", "hooks"))
                    .Then(ValidateDeleteRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the delete request when validation is enabled.")
                        .Tags("validation", "hooks"))
                    .Then(DeleteLoadedEntityAsync, step => step
                        .Named("Delete entity")
                        .Describe("Runs before/after delete hooks and removes the loaded entity from storage.")
                        .Tags("delete", "persistence", "hooks"))
                    .Then(BuildDeleteResponseAsync, step => step
                        .Named("Map response")
                        .Describe("Runs before/after response hooks and maps the deleted entity to the command response.")
                        .Tags("response", "hooks"))
                    .RunAsync(request, cancellationToken);
            }

            var entity = await LoadDeleteEntityAsync(request, cancellationToken);
            await ValidateDeleteRequestAsync(entity, cancellationToken);
            await DeleteLoadedEntityAsync(entity, cancellationToken);
            return await BuildDeleteResponseAsync(entity, cancellationToken);
        }

        private async Task<TEntity> LoadDeleteEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> ValidateDeleteRequestAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(Context.Request, entity, cancellationToken);

            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> DeleteLoadedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeDeleteAsync(Context, cancellationToken);
            await DeleteEntityAsync(entity, cancellationToken);

            await hookStageRunner.AfterDeleteAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TResponse> BuildDeleteResponseAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeResponseAsync(Context, cancellationToken);
            var response = await BuildResponseAsync(Context.Request, entity, cancellationToken);
            Context.Response = response;

            await hookStageRunner.AfterResponseAsync(Context, cancellationToken);

            return response;
        }

        /// <summary>
        /// Retrieves the entity to be deleted based on the request. Throws <see cref="NotFoundException"/> if the entity is not found.
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
        /// Validates the request and entity using the validator adapter if <see cref="ValidateRequest"/> is <c>false</c>.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="entity">The entity to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous validation operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if(!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, entity, cancellationToken);
        }

        /// <summary>
        /// Deletes the entity using the storage adapter.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        protected virtual async Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
            => await EntityDeleteStep.DeleteAsync(Context.Request, entity, cancellationToken);

        /// <summary>
        /// Builds the response after the entity has been deleted.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The deleted entity.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the response as the result.</returns>
        protected virtual ValueTask<TResponse> BuildResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => MapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);
    }

    /// <summary>
    /// Provides a base implementation for delete commands that do not return a response.
    /// </summary>
    public abstract class GenericDeleteCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
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
        /// Gets the storage adapter for deleting entities.
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
        /// Gets the entity delete step.
        /// </summary>
        protected IEntityDeleteStep<TRequest, TEntity> EntityDeleteStep { get; }

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
        protected GenericDeleteCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            EntityLookupStep = Services.GetRequiredService<IEntityLookupStep<TRequest, TEntity, TKey>>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityDeleteStep = Services.GetRequiredService<IEntityDeleteStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity>>();
        }

        /// <summary>
        /// Handles the delete command by retrieving, validating, and deleting the entity.
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
                    .ComposeFlow<TRequest>("Delete command")
                    .Describe("Loads an existing entity, optionally validates the delete request, and deletes the entity.")
                    .Tags("turtlepath", "command", "delete")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(LoadDeleteEntityAsync, step => step
                        .Named("Load entity")
                        .Describe("Runs before/after get entity hooks and loads the entity targeted by the delete request.")
                        .Tags("lookup", "hooks"))
                    .Then(ValidateDeleteRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the delete request when validation is enabled.")
                        .Tags("validation", "hooks"))
                    .Then(DeleteLoadedEntityAsync, step => step
                        .Named("Delete entity")
                        .Describe("Runs before/after delete hooks and removes the loaded entity from storage.")
                        .Tags("delete", "persistence", "hooks"))
                    .RunAsync(request, cancellationToken);

                return;
            }

            var entity = await LoadDeleteEntityAsync(request, cancellationToken);
            await ValidateDeleteRequestAsync(entity, cancellationToken);
            await DeleteLoadedEntityAsync(entity, cancellationToken);
        }

        private async Task<TEntity> LoadDeleteEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;
            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> ValidateDeleteRequestAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(Context.Request, entity, cancellationToken);
            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return entity;
        }

        private async Task<TEntity> DeleteLoadedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeDeleteAsync(Context, cancellationToken);
            await DeleteEntityAsync(entity, cancellationToken);
            await hookStageRunner.AfterDeleteAsync(Context, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Retrieves the entity to delete.
        /// </summary>
        /// <param name="request">The request containing the entity identifier.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The entity to delete.</returns>
        protected virtual async Task<TEntity> GetEntityAsync(TRequest request, CancellationToken cancellationToken)
        {
            return await EntityLookupStep.GetAsync(request, request.Id, cancellationToken);
        }

        /// <summary>
        /// Validates the request using the validator adapter when validation is enabled.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="entity">The entity being deleted.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, entity, cancellationToken);
        }

        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
            => await EntityDeleteStep.DeleteAsync(Context.Request, entity, cancellationToken);
    }
}
