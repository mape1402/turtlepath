namespace TurtlePath.Commands.Steps
{
    using TurtlePath.Validation;

    /// <summary>
    /// Default request validation step.
    /// </summary>
    internal sealed class DefaultRequestValidationStep<TRequest, TEntity> : IRequestValidationStep<TRequest, TEntity>
        where TEntity : class
    {
        private readonly IValidatorAdapter validatorAdapter;

        public DefaultRequestValidationStep(IValidatorAdapter validatorAdapter)
        {
            this.validatorAdapter = validatorAdapter ?? throw new ArgumentNullException(nameof(validatorAdapter));
        }

        public ValueTask ValidateAsync(TRequest request, TEntity entity, CancellationToken cancellationToken)
            => validatorAdapter.ValidateAsync(request, cancellationToken);
    }
}
