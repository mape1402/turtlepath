namespace TurtlePath.Queries
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a query to retrieve multiple TurtlePath BaseEntity instances.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetManyQuery<TEntity, TResponse> : GenericGetManyQuery<TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve multiple TurtlePath BaseEntity instances.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetManyQueryHandler<TQuery, TEntity, TResponse> : GenericGetManyQueryHandler<TQuery, TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
        where TQuery : GetManyQuery<TEntity, TResponse>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetManyQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
