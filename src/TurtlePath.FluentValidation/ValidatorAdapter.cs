namespace TurtlePath.FluentValidation
{
    using global::FluentValidation;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Validation;

    /// <summary>
    /// Provides an implementation of <see cref="IValidatorAdapter"/> using FluentValidation for model validation.
    /// </summary>
    public class ValidatorAdapter : IValidatorAdapter
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAdapter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve validators.</param>
        public ValidatorAdapter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public async ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var validator = serviceProvider.GetService<IValidator<TModel>>();

            if (validator == null)
                throw new InvalidOperationException($"No validator registered for type {typeof(TModel).FullName}");

            var result = await validator.ValidateAsync(model, cancellationToken);

            if (!result.IsValid)
                throw new TurtlePath.Validation.ValidationException(
                    result.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}"));
        }
    }
}
