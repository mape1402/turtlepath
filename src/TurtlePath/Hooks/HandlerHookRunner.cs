namespace TurtlePath.Hooks
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Executes hooks resolved from the current dependency scope.
    /// </summary>
    public interface IHandlerHookRunner
    {
        /// <summary>
        /// Runs all hooks of the specified type in registration order, honoring <see cref="IOrderedHook"/> when present.
        /// </summary>
        /// <typeparam name="THook">The hook interface type to resolve.</typeparam>
        /// <param name="action">The action to execute for each hook.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        ValueTask RunAsync<THook>(Func<THook, ValueTask> action)
            where THook : notnull;
    }

    /// <inheritdoc />
    internal sealed class HandlerHookRunner : IHandlerHookRunner
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve hooks.</param>
        public HandlerHookRunner(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async ValueTask RunAsync<THook>(Func<THook, ValueTask> action)
            where THook : notnull
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var hooks = serviceProvider
                .GetServices<THook>()
                .OrderBy(GetOrder);

            foreach (var hook in hooks)
                await action(hook);
        }

        private int GetOrder<THook>(THook hook)
            => hook is IOrderedHook orderedHook ? orderedHook.Order : 0;
    }
}

