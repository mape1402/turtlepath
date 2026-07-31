namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Requests;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base implementation for handling update commands for TurtlePath BaseEntity instances with CId identifiers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being updated.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseEntityUpdateCommandHandler<TRequest, TResponse, TEntity> : EntityUpdateCommandHandler<TRequest, TResponse, TEntity, CId>
        where TRequest : BaseRequest, IRequest<TResponse>
        where TResponse : BaseResponse
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected BaseEntityUpdateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
