namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Responses;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;
    using System;

    /// <summary>
    /// Provides a base implementation for handling create commands for TurtlePath BaseEntity instances with CId identifiers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being created.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class BaseEntityCreateCommandHandler<TRequest, TResponse, TEntity> : EntityCreateCommandHandler<TRequest, TResponse, TEntity, CId>
        where TRequest : class, IRequest<TResponse>
        where TEntity : BaseEntity
        where TResponse : BaseResponse
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected BaseEntityCreateCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
