namespace TurtlePath.Automations.Handlers
{
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Models.Responses;
    using TurtlePath.Queries;

    internal sealed class AutomatedGetByIdQueryHandler<TQuery, TEntity, TResponse, TKey>
        : GenericGetByIdQueryHandler<TQuery, TEntity, TResponse, TKey>
        where TQuery : GenericGetByIdQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        public AutomatedGetByIdQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    internal sealed class AutomatedGetManyQueryHandler<TQuery, TEntity, TResponse, TKey>
        : GenericGetManyQueryHandler<TQuery, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
        where TQuery : GenericGetManyQuery<TEntity, TResponse, TKey>
    {
        public AutomatedGetManyQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    internal sealed class AutomatedGetPagedQueryHandler<TQuery, TEntity, TResponse, TKey>
        : GenericGetPagedInfoQueryHandler<TQuery, TEntity, TResponse, TKey>
        where TQuery : GenericGetPagedInfoQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        public AutomatedGetPagedQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override string DefaultSorts => null;
    }
}
