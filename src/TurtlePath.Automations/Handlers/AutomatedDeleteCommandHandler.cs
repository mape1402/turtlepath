namespace TurtlePath.Automations.Handlers
{
    using Pelican.Mediator;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Mapping;
    using TurtlePath.Models.Requests;

    internal sealed class AutomatedDeleteCommandHandler<TRequest, TResponse, TEntity, TKey>
        : GenericDeleteCommandHandler<TRequest, TResponse, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IRequest<TResponse>
        where TResponse : class
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedDeleteCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ValueTask<TResponse> BuildResponseAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => MapperAdapter.MapAsync<TEntity, TResponse>(entity, cancellationToken);
    }

    internal sealed class AutomatedDeleteCommandHandler<TRequest, TEntity, TKey>
        : GenericDeleteCommandHandler<TRequest, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IRequest
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedDeleteCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
