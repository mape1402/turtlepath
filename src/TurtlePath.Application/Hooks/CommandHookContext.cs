namespace TurtlePath.Application.Hooks
{
    /// <summary>
    /// Provides shared data for command hooks during a single handler execution.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    public class CommandHookContext<TRequest, TEntity>
    {
        private readonly Dictionary<object, object> _items = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHookContext{TRequest, TEntity}"/> class.
        /// </summary>
        /// <param name="request">The command request being handled.</param>
        public CommandHookContext(TRequest request)
        {
            Request = request;
        }

        /// <summary>
        /// Gets the command request being handled.
        /// </summary>
        public TRequest Request { get; }

        /// <summary>
        /// Gets or sets the entity affected by the command when it is available.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <summary>
        /// Stores a value in the context using the specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key used to store the value.</param>
        /// <param name="value">The value to store.</param>
        public void Set<TValue>(HookContextKey<TValue> key, TValue value)
        {
            EnsureKey(key);

            _items[key] = value;
        }

        /// <summary>
        /// Gets a value from the context using the specified key.
        /// </summary>
        /// <typeparam name="TValue">The expected value type.</typeparam>
        /// <param name="key">The key used to retrieve the value.</param>
        /// <returns>The stored value when found and assignable; otherwise, the default value.</returns>
        public TValue Get<TValue>(HookContextKey<TValue> key)
        {
            EnsureKey(key);

            return _items.TryGetValue(key, out var value) && value is TValue typedValue
                ? typedValue
                : default;
        }

        /// <summary>
        /// Determines whether the context contains a value for the specified key.
        /// </summary>
        /// <typeparam name="TValue">The expected value type.</typeparam>
        /// <param name="key">The key used to find the value.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool Has<TValue>(HookContextKey<TValue> key)
        {
            EnsureKey(key);

            return _items.ContainsKey(key);
        }

        /// <summary>
        /// Tries to get a value from the context using the specified key.
        /// </summary>
        /// <typeparam name="TValue">The expected value type.</typeparam>
        /// <param name="key">The key used to retrieve the value.</param>
        /// <param name="value">The retrieved value when found and assignable; otherwise, the default value.</param>
        /// <returns>True if the value was found and assignable; otherwise, false.</returns>
        public bool TryGet<TValue>(HookContextKey<TValue> key, out TValue value)
        {
            EnsureKey(key);

            if (_items.TryGetValue(key, out var item) && item is TValue typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        private static void EnsureKey<TValue>(HookContextKey<TValue> key)
        {
            if (key.IsDefault)
                throw new ArgumentException("Hook context key cannot be default.", nameof(key));
        }
    }

    /// <summary>
    /// Provides shared data for command hooks during a single handler execution that returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the command request.</typeparam>
    /// <typeparam name="TEntity">The type of the entity affected by the command.</typeparam>
    /// <typeparam name="TResponse">The type of the command response.</typeparam>
    public class CommandHookContext<TRequest, TEntity, TResponse> : CommandHookContext<TRequest, TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHookContext{TRequest, TEntity, TResponse}"/> class.
        /// </summary>
        /// <param name="request">The command request being handled.</param>
        public CommandHookContext(TRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Gets or sets the command response when it is available.
        /// </summary>
        public TResponse Response { get; set; }
    }
}
