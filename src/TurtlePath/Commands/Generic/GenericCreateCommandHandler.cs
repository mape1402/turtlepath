namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using Spider.Pipelines.Core;
    using System;
    using System.Linq.Expressions;
    using TurtlePath.Commands.Steps;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides a base implementation for handling create commands that return a response, including validation, mapping, saving, and response mapping.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being created.</typeparam>
    /// <typeparam name="TKey">The entity identifier type.</typeparam>
    public abstract class GenericCreateCommandHandler<TRequest, TResponse, TEntity, TKey> : BaseCommandHandler<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
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
        /// Gets the request validation step.
        /// </summary>
        protected IRequestValidationStep<TRequest, TEntity> ValidationStep { get; }

        /// <summary>
        /// Gets the entity creation step.
        /// </summary>
        protected IEntityCreationStep<TRequest, TEntity> EntityCreationStep { get; }

        /// <summary>
        /// Gets the entity add step.
        /// </summary>
        protected IEntityAddStep<TRequest, TEntity> EntityAddStep { get; }

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
        protected GenericCreateCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityCreationStep = Services.GetRequiredService<IEntityCreationStep<TRequest, TEntity>>();
            EntityAddStep = Services.GetRequiredService<IEntityAddStep<TRequest, TEntity>>();
            ResponseMappingStep = Services.GetRequiredService<IResponseMappingStep<TRequest, TEntity, TResponse, TKey>>();
            ResponseOptions = Services.GetService<ICommandResponseOptions<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity, TResponse>>();
        }

        /// <summary>
        /// Handles the create command by validating, mapping, saving, and returning the response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation, with the response for the create command as the result.</returns>
        public override async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default)
        {
            Context = new CommandHookContext<TRequest, TEntity, TResponse>(request);

            if (Spider != null)
            {
                return await Spider
                    .ComposeFlow<TRequest, TResponse>("Create command")
                    .Describe("Validates a create request, maps it to a new entity, saves it, and maps the persisted entity to a response.")
                    .Tags("turtlepath", "command", "create")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(ValidateCreateRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the incoming create request.")
                        .Tags("validation", "hooks"))
                    .Then(MapCreateRequestAsync, step => step
                        .Named("Map entity")
                        .Describe("Runs before/after map hooks and creates the entity from the request.")
                        .Tags("mapping", "hooks"))
                    .Then(SaveCreatedEntityAsync, step => step
                        .Named("Save entity")
                        .Describe("Runs before/after save hooks and persists the created entity.")
                        .Tags("persistence", "hooks"))
                    .Then(BuildCreateResponseAsync, step => step
                        .Named("Map response")
                        .Describe("Runs before/after response hooks and maps the saved entity to the command response.")
                        .Tags("response", "projection", "hooks"))
                    .RunAsync(request, cancellationToken);
            }

            await ValidateCreateRequestAsync(request, cancellationToken);
            var entity = await MapCreateRequestAsync(request, cancellationToken);
            await SaveCreatedEntityAsync(entity, cancellationToken);
            return await BuildCreateResponseAsync(entity, cancellationToken);
        }

        private async Task<TRequest> ValidateCreateRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, cancellationToken);

            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return request;
        }

        private async Task<TEntity> MapCreateRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            var entity = await MapToEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await hookStageRunner.AfterMapAsync(Context, cancellationToken);
            return entity;
        }

        private async Task SaveCreatedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await SaveEntityAsync(Context.Request, entity, cancellationToken);

            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);
        }

        private async Task<TResponse> BuildCreateResponseAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeResponseAsync(Context, cancellationToken);
            var response = await MapToResponseAsync(Context.Request, entity, cancellationToken);
            Context.Response = response;

            await hookStageRunner.AfterResponseAsync(Context, cancellationToken);

            return response;
        }

        /// <summary>
        /// Validates the request using the validator adapter if <see cref="ValidateRequest"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous validation operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, Context?.Entity, cancellationToken);
        }

        /// <summary>
        /// Maps the request to an entity using the mapper adapter.
        /// </summary>
        /// <param name="request">The request to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped entity as the result.</returns>
        protected virtual ValueTask<TEntity> MapToEntityAsync(TRequest request, CancellationToken cancellationToken)
            => EntityCreationStep.CreateAsync(request, cancellationToken);

        /// <summary>
        /// Saves the entity using the storage adapter.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        protected virtual async Task SaveEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => await EntityAddStep.AddAsync(request, entity, cancellationToken);

        /// <summary>
        /// Maps the entity to a response using the mapper adapter or retrieves a projection from storage if <see cref="UseProjectionFromStorage"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to map to a response.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped response as the result.</returns>
        protected virtual async ValueTask<TResponse> MapToResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => await ResponseMappingStep.MapAsync(
                request,
                entity,
                UseProjectionFromStorage,
                EntityKeyExpression.Equals<TEntity, TKey>(entity.Id),
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
    /// Provides a base implementation for create commands that do not return a response.
    /// </summary>
    public abstract class GenericCreateCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
        where TRequest : class, IRequest
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
        /// Gets the validator adapter for validating requests.
        /// </summary>
        protected IValidatorAdapter ValidatorAdapter { get; }

        /// <summary>
        /// Gets the mapper adapter for mapping between types.
        /// </summary>
        protected IMapperAdapter MapperAdapter { get; }

        /// <summary>
        /// Gets the request validation step.
        /// </summary>
        protected IRequestValidationStep<TRequest, TEntity> ValidationStep { get; }

        /// <summary>
        /// Gets the entity creation step.
        /// </summary>
        protected IEntityCreationStep<TRequest, TEntity> EntityCreationStep { get; }

        /// <summary>
        /// Gets the entity add step.
        /// </summary>
        protected IEntityAddStep<TRequest, TEntity> EntityAddStep { get; }

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
        protected GenericCreateCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Spider = Services.GetService<ISpider>();
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
            ValidationStep = Services.GetRequiredService<IRequestValidationStep<TRequest, TEntity>>();
            EntityCreationStep = Services.GetRequiredService<IEntityCreationStep<TRequest, TEntity>>();
            EntityAddStep = Services.GetRequiredService<IEntityAddStep<TRequest, TEntity>>();
            hookStageRunner = Services.GetRequiredService<ICommandHookStageRunner<TRequest, TEntity>>();
        }

        /// <summary>
        /// Handles the create command by validating, mapping, and saving the entity.
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
                    .ComposeFlow<TRequest>("Create command")
                    .Describe("Validates a create request, maps it to a new entity, and saves it.")
                    .Tags("turtlepath", "command", "create")
                    .UsingProfile(TurtlePathCommandFlowProfiles.Command)
                    .Then(ValidateCreateRequestAsync, step => step
                        .Named("Validate request")
                        .Describe("Runs before/after validation hooks and validates the incoming create request.")
                        .Tags("validation", "hooks"))
                    .Then(MapCreateRequestAsync, step => step
                        .Named("Map entity")
                        .Describe("Runs before/after map hooks and creates the entity from the request.")
                        .Tags("mapping", "hooks"))
                    .Then(SaveCreatedEntityAsync, step => step
                        .Named("Save entity")
                        .Describe("Runs before/after save hooks and persists the created entity.")
                        .Tags("persistence", "hooks"))
                    .RunAsync(request, cancellationToken);

                return;
            }

            await ValidateCreateRequestAsync(request, cancellationToken);
            var entity = await MapCreateRequestAsync(request, cancellationToken);
            await SaveCreatedEntityAsync(entity, cancellationToken);
        }

        private async Task<TRequest> ValidateCreateRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, cancellationToken);
            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);
            return request;
        }

        private async Task<TEntity> MapCreateRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            var entity = await MapToEntityAsync(request, cancellationToken);
            Context.Entity = entity;
            await hookStageRunner.AfterMapAsync(Context, cancellationToken);
            return entity;
        }

        private async Task SaveCreatedEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await SaveEntityAsync(Context.Request, entity, cancellationToken);
            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);
        }

        /// <summary>
        /// Validates the request using the validator adapter when validation is enabled.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual ValueTask ValidateAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidationStep.ValidateAsync(request, Context?.Entity, cancellationToken);
        }

        /// <summary>
        /// Maps the request to an entity.
        /// </summary>
        /// <param name="request">The request to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The mapped entity.</returns>
        protected virtual ValueTask<TEntity> MapToEntityAsync(TRequest request, CancellationToken cancellationToken)
            => EntityCreationStep.CreateAsync(request, cancellationToken);

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async Task SaveEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => await EntityAddStep.AddAsync(request, entity, cancellationToken);
    }
}
