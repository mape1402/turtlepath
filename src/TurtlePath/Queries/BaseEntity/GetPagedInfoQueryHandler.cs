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
    public abstract class GetPagedInfoQuery<TEntity, TResponse> : GenericGetPagedInfoQuery<TEntity, TResponse, CId>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="pagedSettings">The paged settings for the query.</param>
        protected GetPagedInfoQuery(PagedSettings pagedSettings) : base(pagedSettings)
        {
        }
    }

    /// <summary>
    /// Provides a base implementation for handling paged queries for TurtlePath BaseEntity instances.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class GetPagedInfoQueryHandler<TQuery, TEntity, TResponse> : GenericGetPagedInfoQueryHandler<TQuery, TEntity, TResponse, CId>
        where TQuery : GetPagedInfoQuery<TEntity, TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected GetPagedInfoQueryHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
