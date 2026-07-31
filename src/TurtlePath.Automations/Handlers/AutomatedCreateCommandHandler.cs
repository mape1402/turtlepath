namespace TurtlePath.Automations.Handlers
{
    using Pelican.Mediator;
    using TurtlePath.Commands;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;

    internal sealed class AutomatedCreateCommandHandler<TRequest, TResponse, TEntity, TKey>
        : GenericCreateCommandHandler<TRequest, TResponse, TEntity, TKey>
        where TRequest : class, IRequest<TResponse>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        public AutomatedCreateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    internal sealed class AutomatedCreateCommandHandler<TRequest, TEntity, TKey>
        : GenericCreateCommandHandler<TRequest, TEntity, TKey>
        where TRequest : class, IRequest
        where TEntity : class, IEntity<TKey>
    {
        public AutomatedCreateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
