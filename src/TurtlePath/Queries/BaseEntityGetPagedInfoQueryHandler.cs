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

    /// <summary>
    /// Represents a query to retrieve a paged set of TurtlePath BaseEntity instances.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseEntityGetPagedInfoQuery<TEntity, TResponse> : EntityGetPagedInfoQuery<TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="pagedSettings">The paged settings for the query.</param>
        protected BaseEntityGetPagedInfoQuery(PagedSettings pagedSettings) : base(pagedSettings)
        {
        }
    }

    /// <summary>
    /// Provides a base implementation for handling paged queries for TurtlePath BaseEntity instances.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseEntityGetPagedInfoQueryHandler<TQuery, TEntity, TResponse> : EntityGetPagedInfoQueryHandler<TQuery, TEntity, TResponse, CId>
        where TQuery : BaseEntityGetPagedInfoQuery<TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected BaseEntityGetPagedInfoQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
