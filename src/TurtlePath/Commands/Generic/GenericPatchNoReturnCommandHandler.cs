namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Requests;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides a base implementation for patch commands that do not return a response.
    /// </summary>
    public abstract class GenericPatchNoReturnCommandHandler<TRequest, TEntity, TKey> : NoReturnCommandHandler<TRequest>
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
        protected GenericPatchNoReturnCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
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
            return await StorageReaderAdapter
                .For<TEntity>()
                .AsTracking()
                .Where(EntityKeyExpression.Equals<TEntity, TKey>(request.Id))
                .FirstOrDefaultAsync<TEntity>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id?.ToString());
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

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Applies request changes to the entity.
        /// </summary>
        /// <param name="request">The request containing patch data.</param>
        /// <param name="entity">The entity to patch.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract ValueTask PatchEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the patched entity.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task UpdateEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => StorageWriterAdapter.SaveChangesAsync(cancellationToken);
    }
}
