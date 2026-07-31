namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

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
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => true;

        private readonly ICommandHookStageRunner<TRequest, TEntity, TResponse> hookStageRunner;

        /// <summary>
        /// Gets a value indicating whether to use a projection from storage for the response mapping.
        /// </summary>
        protected virtual bool UseProjectionFromStorage => false;

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
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
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

            await hookStageRunner.BeforeGetEntityAsync(Context, cancellationToken);
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await hookStageRunner.AfterGetEntityAsync(Context, cancellationToken);

            await hookStageRunner.BeforeValidationAsync(Context, cancellationToken);
            await ValidateAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterValidationAsync(Context, cancellationToken);

            await hookStageRunner.BeforeMapAsync(Context, cancellationToken);
            await MapEntityAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterMapAsync(Context, cancellationToken);

            await hookStageRunner.BeforeSaveAsync(Context, cancellationToken);
            await UpdateEntityAsync(request, entity, cancellationToken);

            await hookStageRunner.AfterSaveAsync(Context, cancellationToken);

            await hookStageRunner.BeforeResponseAsync(Context, cancellationToken);
            var response = await MapToResponseAsync(request, entity, cancellationToken);
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
            return await StorageReaderAdapter
                .For<TEntity>()
                .AsTracking()
                .Where(EntityKeyExpression.Equals<TEntity, TKey>(request.Id))
                .FirstOrDefaultAsync<TEntity>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id.ToString());
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
        protected virtual ValueTask MapEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => MapperAdapter.UpdateMapAsync(request, entity, cancellationToken);

        /// <summary>
        /// Updates the entity in the storage using the storage adapter.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        protected virtual Task UpdateEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => StorageWriterAdapter.SaveChangesAsync(cancellationToken);

        /// <summary>
        /// Maps the updated entity to a response using the mapper adapter or retrieves a projection from storage if <see cref="UseProjectionFromStorage"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The updated entity to map to a response.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped response as the result.</returns>
        protected virtual async ValueTask<TResponse> MapToResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => UseProjectionFromStorage
                ? await StorageReaderAdapter
                    .For<TEntity>()
                    .AsNoTracking()
                    .Where(EntityKeyExpression.Equals<TEntity, TKey>(request.Id))
                    .FirstOrDefaultAsync<TResponse>(cancellationToken)
                : await MapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);
    }
}
