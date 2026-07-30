namespace TurtlePath.Application.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Application.Exceptions;
    using TurtlePath.Application.Hooks;
    using TurtlePath.Application.Models.Requests;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Contracts;
    using Pelican.Mediator;

    /// <summary>
    /// Provides a base implementation for handling delete commands that return a response, including entity retrieval, validation, deletion, and response building.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being deleted.</typeparam>
    public abstract class DeleteCommandHandler<TRequest, TResponse, TEntity> : BaseCommandHandler<TRequest, TResponse>
        where TRequest : BaseRequest, IRequest<TResponse>
        where TResponse : class
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

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
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => false;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity, TResponse> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommandHandler{TRequest, TResponse, TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected DeleteCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = serviceProvider.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = serviceProvider.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = serviceProvider.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = serviceProvider.GetRequiredService<IMapperAdapter>();
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

            await Services.RunHooksAsync<IBeforeGetEntityHook<TRequest, TEntity>>(
                hook => hook.BeforeGetEntityAsync(Context, cancellationToken));
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await Services.RunHooksAsync<IAfterGetEntityHook<TRequest, TEntity>>(
                hook => hook.AfterGetEntityAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(Context, cancellationToken));
            await ValidateAsync(request, entity, cancellationToken);    

            await Services.RunHooksAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeDeleteHook<TRequest, TEntity>>(
                hook => hook.BeforeDeleteAsync(Context, cancellationToken));
            await DeleteEntityAsync(entity, cancellationToken);

            await Services.RunHooksAsync<IAfterDeleteHook<TRequest, TEntity>>(
                hook => hook.AfterDeleteAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.BeforeResponseAsync(Context, cancellationToken));
            var response = await BuildResponseAsync(request, entity, cancellationToken);
            Context.Response = response;

            await Services.RunHooksAsync<IAfterResponseHook<TRequest, TEntity, TResponse>>(
                hook => hook.AfterResponseAsync(Context, cancellationToken));

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
            return await StorageReaderAdapter
                .For<TEntity>()
                .AsTracking()
                .Where(e => e.Id == request.Id)
                .FirstOrDefaultAsync<TEntity>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id.ToString());
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

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Deletes the entity using the storage adapter.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        protected virtual async Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            StorageWriterAdapter.Remove(entity);
            await StorageWriterAdapter.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Builds the response after the entity has been deleted.
        /// </summary>
        /// <param name="request">The request associated with the entity.</param>
        /// <param name="entity">The deleted entity.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous operation, with the response as the result.</returns>
        protected abstract ValueTask<TResponse> BuildResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides a base implementation for handling delete commands that do not return a response, including entity retrieval, validation, and deletion.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being deleted.</typeparam>
    public abstract class DeleteCommandHandler<TRequest, TEntity> : NoReturnCommandHandler<TRequest>
        where TRequest : BaseRequest, IRequest
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Gets the service provider used to resolve dependencies.
        /// </summary>
        protected IServiceProvider Services { get; }

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
        /// Gets a value indicating whether the request should be validated before processing.
        /// </summary>
        protected virtual bool ValidateRequest => false;

        /// <summary>
        /// Gets the hook context for the current handler execution.
        /// </summary>
        protected CommandHookContext<TRequest, TEntity> Context { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommandHandler{TRequest, TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected DeleteCommandHandler(IServiceProvider serviceProvider)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            StorageWriterAdapter = serviceProvider.GetRequiredService<IStorageWriterAdapter>();
            StorageReaderAdapter = serviceProvider.GetRequiredService<IStorageReaderAdapter>();
            ValidatorAdapter = serviceProvider.GetRequiredService<IValidatorAdapter>();
            MapperAdapter = serviceProvider.GetRequiredService<IMapperAdapter>();
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

            await Services.RunHooksAsync<IBeforeGetEntityHook<TRequest, TEntity>>(
                hook => hook.BeforeGetEntityAsync(Context, cancellationToken));
            var entity = await GetEntityAsync(request, cancellationToken);
            Context.Entity = entity;

            await Services.RunHooksAsync<IAfterGetEntityHook<TRequest, TEntity>>(
                hook => hook.AfterGetEntityAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeValidationHook<TRequest, TEntity>>(
                hook => hook.BeforeValidationAsync(Context, cancellationToken));
            await ValidateAsync(request, entity, cancellationToken);

            await Services.RunHooksAsync<IAfterValidationHook<TRequest, TEntity>>(
                hook => hook.AfterValidationAsync(Context, cancellationToken));

            await Services.RunHooksAsync<IBeforeDeleteHook<TRequest, TEntity>>(
                hook => hook.BeforeDeleteAsync(Context, cancellationToken));
            await DeleteEntityAsync(entity, cancellationToken);

            await Services.RunHooksAsync<IAfterDeleteHook<TRequest, TEntity>>(
                hook => hook.AfterDeleteAsync(Context, cancellationToken));
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
            return await StorageReaderAdapter
                .For<TEntity>()
                .AsTracking()
                .Where(e => e.Id == request.Id)
                .FirstOrDefaultAsync<TEntity>(cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id.ToString());
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
            if (!ValidateRequest)
                return ValueTask.CompletedTask;

            return ValidatorAdapter.ValidateAsync(request, cancellationToken);
        }

        /// <summary>
        /// Deletes the entity using the storage adapter.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        protected virtual async Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            StorageWriterAdapter.Remove(entity);
            await StorageWriterAdapter.SaveChangesAsync(cancellationToken);
        }
    }
}
