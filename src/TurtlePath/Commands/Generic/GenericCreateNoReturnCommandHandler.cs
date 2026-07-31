namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides a base implementation for create commands that do not return a response.
    /// </summary>
    public abstract class GenericCreateNoReturnCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
        where TRequest : class, IRequest
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
        /// Gets the validator adapter for validating requests.
        /// </summary>
        protected IValidatorAdapter ValidatorAdapter { get; }

        /// <summary>
        /// Gets the mapper adapter for mapping between types.
        /// </summary>
        protected IMapperAdapter MapperAdapter { get; }

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
        protected GenericCreateNoReturnCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
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

            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, cancellationToken);
            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);

            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            var entity = await MapToEntityAsync(request, cancellationToken);
            Context.Entity = entity;
            await hookStageRunner.AfterMapAsync(Context, cancellationToken);

            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await SaveEntityAsync(request, entity, cancellationToken);
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

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Maps the request to an entity.
        /// </summary>
        /// <param name="request">The request to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>The mapped entity.</returns>
        protected virtual ValueTask<TEntity> MapToEntityAsync(TRequest request, CancellationToken cancellationToken)
            => MapperAdapter.MapAsync<TRequest, TEntity>(request, cancellationToken);

        /// <summary>
        /// Saves the entity.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual async Task SaveEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            await StorageWriterAdapter.AddAsync(entity, cancellationToken);
            await StorageWriterAdapter.SaveChangesAsync(cancellationToken);
        }
    }
}
