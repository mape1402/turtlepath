namespace TurtlePath.Automations.Handlers
{
    using Pelican.Mediator;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    internal sealed class AutomatedUpdateCommandHandler<TRequest, TResponse, TEntity, TKey>
        : GenericUpdateCommandHandler<TRequest, TResponse, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IRequest<TResponse>
        where TResponse : class, IBaseResponse<TKey>
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedUpdateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    internal sealed class AutomatedUpdateCommandHandler<TRequest, TEntity, TKey>
        : GenericUpdateCommandHandler<TRequest, TEntity, TKey>
        where TRequest : class, IBaseRequest<TKey>, IRequest
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedUpdateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
