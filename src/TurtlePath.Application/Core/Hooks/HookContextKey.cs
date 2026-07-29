namespace TurtlePath.Core.Hooks
{
    /// <summary>
    /// Represents a typed key used to share values between hooks and handler extension points.
    /// </summary>
    /// <typeparam name="TValue">The type of value associated with the key.</typeparam>
    public readonly struct HookContextKey<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookContextKey{TValue}"/> struct.
        /// </summary>
        /// <param name="name">The descriptive name of the key.</param>
        public HookContextKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Hook context key name cannot be empty.", nameof(name));

            Name = name;
        }

        /// <summary>  
        /// Gets the descriptive name of the key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the key was created with the default struct value.
        /// </summary>
        public bool IsDefault
            => string.IsNullOrWhiteSpace(Name);

        /// <inheritdoc/>
        public override string ToString()
            => Name;
    }

    /// <summary>
    /// Provides factory methods for hook context keys.
    /// </summary>
    public static class HookContextKey
    {
        /// <summary>
        /// Creates a typed key used to share values between hooks and handler extension points.
        /// </summary>
        /// <typeparam name="TValue">The type of value associated with the key.</typeparam>
        /// <param name="name">The descriptive name of the key.</param>
        /// <returns>A typed hook context key.</returns>
        public static HookContextKey<TValue> Set<TValue>(string name)
            => new(name);
    }
}
