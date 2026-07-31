namespace TurtlePath.Queries
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System.Linq.Expressions;

    /// <summary>
    /// Represents a query to retrieve a single TurtlePath BaseEntity by a specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value used to retrieve the entity.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetOneQuery<TValue, TEntity, TResponse> : GenericGetOneQuery<TValue, TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
    }

    /// <summary>
    /// Provides a base implementation for handling queries that retrieve a single TurtlePath BaseEntity by a specified value.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TValue">The type of the value used to retrieve the entity.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetOneQueryHandler<TQuery, TValue, TEntity, TResponse> : GenericGetOneQueryHandler<TQuery, TValue, TEntity, TResponse, CId>
        where TQuery : GetOneQuery<TValue, TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetOneQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
