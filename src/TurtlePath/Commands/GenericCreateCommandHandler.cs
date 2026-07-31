namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System;

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
        protected virtual bool ValidateRequest => true;

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
        protected GenericCreateCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = Services.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = Services.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = Services.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = Services.GetRequiredService<IMapperAdapter>();
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

            await Services.RunHooksAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(Context, cancellationToken));
            await ValidateAsync(request, cancellationToken);

            await Services.RunHooksAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeMapHook<TRequest, TEntity>>(
                hook => hook.BeforeMapAsync(Context, cancellationToken));
            var entity = await MapToEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await Services.RunHooksAsync<IAfterMapHook<TRequest, TEntity>>(
                hook => hook.AfterMapAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeSaveHook<TRequest, TEntity>>(
                hook => hook.BeforeSaveAsync(Context, cancellationToken));
            await SaveEntityAsync(request, entity, cancellationToken);

            await Services.RunHooksAsync<IAfterSaveHook<TRequest, TEntity>>(
                hook => hook.AfterSaveAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.BeforeResponseAsync(Context, cancellationToken));
            var response = await MapToResponseAsync(request, entity, cancellationToken);
            Context.Response = response;

            await Services.RunHooksAsync<IAfterResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.AfterResponseAsync(Context, cancellationToken));

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

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Maps the request to an entity using the mapper adapter.
        /// </summary>
        /// <param name="request">The request to map.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped entity as the result.</returns>
        protected virtual ValueTask<TEntity> MapToEntityAsync(TRequest request, CancellationToken cancellationToken)
            => MapperAdapter.MapAsync<TRequest, TEntity>(request, cancellationToken);

        /// <summary>
        /// Saves the entity using the storage adapter.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous save operation.</returns>
        protected virtual async Task SaveEntityAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
        {
            await StorageWriterAdapter.AddAsync(entity, cancellationToken);
            await StorageWriterAdapter.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Maps the entity to a response using the mapper adapter or retrieves a projection from storage if <see cref="UseProjectionFromStorage"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The entity to map to a response.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous mapping operation, with the mapped response as the result.</returns>
        protected virtual async ValueTask<TResponse> MapToResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => UseProjectionFromStorage ?
               await StorageReaderAdapter
                   .For<TEntity>()
                   .AsNoTracking()
                   .Where(EntityKeyExpression.Equals<TEntity, TKey>(entity.Id))
                   .FirstOrDefaultAsync<TResponse>(cancellationToken) :
               await MapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);
    }
}
