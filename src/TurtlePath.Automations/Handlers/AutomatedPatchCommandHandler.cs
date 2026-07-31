namespace TurtlePath.Automations.Handlers
{
    using Pelican.Mediator;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    internal sealed class AutomatedPatchCommandHandler<TRequest, TResponse, TEntity, TKey>
        : GenericPatchCommandHandler<TRequest, TResponse, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IPatchAction<TEntity>, IRequest<TResponse>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        public AutomatedPatchCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ValueTask<TResponse> BuildResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => MapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);
    }

    internal sealed class AutomatedPatchCommandHandler<TRequest, TEntity, TKey>
        : GenericPatchCommandHandler<TRequest, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IPatchAction<TEntity>, IRequest
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedPatchCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
