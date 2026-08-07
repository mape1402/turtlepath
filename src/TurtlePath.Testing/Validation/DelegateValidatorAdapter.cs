namespace TurtlePath.Testing.Validation
{
    using TurtlePath.Validation;

    /// <summary>
    /// Validator adapter backed by explicitly registered delegates for tests.
    /// </summary>
    public sealed class DelegateValidatorAdapter : IValidatorAdapter
    {
        private readonly Dictionary<Type, Func<object, CancellationToken, ValueTask>> validators = [];

        /// <summary>
        /// Gets or sets whether requests without explicit validators should be treated as valid.
        /// </summary>
        public bool AllowMissingValidators { get; set; } = true;

        /// <summary>
        /// Registers a valid request model.
        /// </summary>
        public DelegateValidatorAdapter WithValidModel<TModel>()
        {
            validators[typeof(TModel)] = (_, _) => ValueTask.CompletedTask;
            return this;
        }

        /// <summary>
        /// Registers a validation delegate.
        /// </summary>
        public DelegateValidatorAdapter WithValidator<TModel>(Func<TModel, CancellationToken, ValueTask> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            validators[typeof(TModel)] = (model, cancellationToken) =>
                validator((TModel)model, cancellationToken);

            return this;
        }

        /// <inheritdoc />
        public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            if (validators.TryGetValue(typeof(TModel), out var validator))
                return validator(model, cancellationToken);

            if (AllowMissingValidators)
                return ValueTask.CompletedTask;

            throw new InvalidOperationException($"Validator for {typeof(TModel).Name} is not configured.");
        }
    }
}
