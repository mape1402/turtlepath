namespace TurtlePath.Commands
{
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Exceptions;
    using TurtlePath.Hooks;
    using TurtlePath.Models.Requests;
    using TurtlePath.Persistence;
    using TurtlePath.Validation;
    using TurtlePath.Mapping;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Domain.Identifier;
    using Pelican.Mediator;

    /// <summary>
    /// Provides a base implementation for handling delete commands for TurtlePath BaseEntity instances with CId identifiers.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity being deleted.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class DeleteCommandHandler<TRequest, TResponse, TEntity> : GenericDeleteCommandHandler<TRequest, TResponse, TEntity, CId>
        where TRequest : BaseRequest, IRequest<TResponse>
        where TResponse : class
        where TEntity : BaseEntity
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        protected DeleteCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
