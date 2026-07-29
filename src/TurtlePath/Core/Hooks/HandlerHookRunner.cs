namespace TurtlePath.Core.Hooks
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Executes hooks resolved from a service provider.
    /// </summary>
    public static class HandlerHookRunner
    {
        /// <summary>
        /// Runs all hooks of the specified type in registration order, honoring <see cref="IOrderedHook"/> when present.
        /// </summary>
        /// <typeparam name="THook">The hook interface type to resolve.</typeparam>
        /// <param name="serviceProvider">The service provider used to resolve hooks.</param>
        /// <param name="action">The action to execute for each hook.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async ValueTask RunHooksAsync<THook>(this IServiceProvider serviceProvider, Func<THook, ValueTask> action)
            where THook : notnull
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var hooks = serviceProvider
                .GetServices<THook>()
                .OrderBy(GetOrder);

            foreach (var hook in hooks)
                await action(hook);
        }

        private static int GetOrder<THook>(THook hook)
            => hook is IOrderedHook orderedHook ? orderedHook.Order : 0;
    }
}
