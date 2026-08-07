namespace TurtlePath.Testing
{
    using Microsoft.Extensions.DependencyInjection;
    using Pelican.Mediator;
    using TurtlePath.Domain.Contracts;
    using TurtlePath.Testing.Persistence;

    /// <summary>
    /// Runtime test host for resolving services and dispatching Pelican requests.
    /// </summary>
    public sealed partial class TurtlePathTestHost : IAsyncDisposable, IDisposable
    {
        private readonly ServiceProvider provider;
        private readonly IServiceScope scope;

        internal TurtlePathTestHost(ServiceProvider provider)
        {
            this.provider = provider;
            scope = provider.CreateScope();
        }

        /// <summary>
        /// Gets the scoped service provider used by the test.
        /// </summary>
        public IServiceProvider Services => scope.ServiceProvider;

        /// <summary>
        /// Gets the in-memory storage registered in this host.
        /// </summary>
        public InMemoryTurtlePathStorage Storage => Services.GetRequiredService<InMemoryTurtlePathStorage>();

        /// <summary>
        /// Creates a new test host builder.
        /// </summary>
        public static TurtlePathTestHostBuilder Create() => new();

        /// <summary>
        /// Resolves a service from the test scope.
        /// </summary>
        public TService Resolve<TService>()
            where TService : notnull
            => Services.GetRequiredService<TService>();

        /// <summary>
        /// Gets a typed in-memory entity set.
        /// </summary>
        public InMemoryEntitySet<TEntity> Store<TEntity>()
            where TEntity : class, IEntity
            => Storage.Set<TEntity>();

        /// <summary>
        /// Sends a Pelican request that returns a response.
        /// </summary>
        public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Resolve<IMediator>().Send(request, cancellationToken);

        /// <summary>
        /// Sends a Pelican request that does not return a response.
        /// </summary>
        public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)
            => Resolve<IMediator>().Send(request, cancellationToken);

        /// <inheritdoc />
        public void Dispose()
        {
            scope.Dispose();
            provider.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (scope is IAsyncDisposable asyncScope)
                await asyncScope.DisposeAsync();
            else
                scope.Dispose();

            await provider.DisposeAsync();
        }
    }
}
