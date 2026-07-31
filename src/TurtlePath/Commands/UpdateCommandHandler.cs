namespace TurtlePath.Commands
{
    using Pelican.Mediator;
    using System;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;

    /// <summary>
    /// Provides a base implementation for handling update commands for TurtlePath BaseEntity instances with CId identifiers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being updated.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class UpdateCommandHandler<TRequest, TResponse, TEntity> : GenericUpdateCommandHandler<TRequest, TResponse, TEntity, CId>
        where TRequest : BaseRequest, IRequest<TResponse>
        where TResponse : BaseResponse
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected UpdateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }

    /// <summary>
    /// Provides a base implementation for update commands that do not return a response for TurtlePath BaseEntity instances with CId identifiers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being updated.</typeparam>
    public abstract class UpdateCommandHandler<TRequest, TEntity> : GenericUpdateCommandHandler<TRequest, TEntity, CId>
        where TRequest : BaseRequest, IRequest
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected UpdateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
