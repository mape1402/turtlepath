namespace TurtlePath.Application.Hooks
{
    /// <summary>
    /// Provides shared data for query hooks during a single handler execution.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query request.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public class QueryHookContext<TQuery, TResult>
    {
        private readonly Dictionary<object, object> _items = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHookContext{TQuery, TResult}"/> class.
        /// </summary>
        /// <param name="query">The query request being handled.</param>
        public QueryHookContext(TQuery query)
        {
            Query = query;
        }

        /// <summary>
        /// Gets the query request being handled.
        /// </summary>
        public TQuery Query { get; }

        /// <summary>
        /// Gets or sets the query result when it is available.
        /// </summary>
        public TResult Result { get; set; }

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
}
