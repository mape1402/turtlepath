namespace TurtlePath.Automations.Handlers
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq.Expressions;
    using TurtlePath.Automations.Descriptors;
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

    internal sealed class AutomatedGetOneQueryHandler<TQuery, TValue, TEntity, TResponse, TKey>
        : GenericGetOneQueryHandler<TQuery, TValue, TEntity, TResponse, TKey>
        where TQuery : GenericGetOneQuery<TValue, TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        public AutomatedGetOneQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override Expression<Func<TEntity, bool>> GetFilterExpression(TQuery request)
        {
            var value = request.Value;
            return entity => entity.Id.Equals(value);
        }
    }

    internal sealed class AutomatedGetPagedQueryHandler<TQuery, TEntity, TResponse, TKey>
        : GenericGetPagedInfoQueryHandler<TQuery, TEntity, TResponse, TKey>
        where TQuery : GenericGetPagedInfoQuery<TEntity, TResponse, TKey>
        where TEntity : class, IEntity<TKey>
        where TResponse : class, IBaseResponse<TKey>
    {
        private readonly AutomationDescriptor descriptor;

        public AutomatedGetPagedQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            descriptor = serviceProvider
                .GetRequiredService<AutomationDescriptorRegistry>()
                .Find(typeof(TQuery), typeof(PagedResponse<TResponse>));
        }

        protected override string DefaultSorts => descriptor?.DefaultSortProperty;
    }
}
