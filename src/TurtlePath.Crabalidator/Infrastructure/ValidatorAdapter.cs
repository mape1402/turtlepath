namespace TurtlePath.Core.Infrastructure
{
    using Crabalidator;
    using Microsoft.Extensions.DependencyInjection;
    using TurtlePath.Core.Exceptions;
    using TurtlePath.Core.Services;

    /// <summary>
    /// Provides an implementation of <see cref="IValidatorAdapter"/> using Crabalidator for model validation.
    /// </summary>
    public class ValidatorAdapter : IValidatorAdapter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAdapter"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve validators.</param>
        public ValidatorAdapter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Asynchronously validates the specified model using a registered Crabalidator validator.
        /// Throws <see cref="InvalidOperationException"/> if no validator is registered for the model type.
        /// </summary>
        /// <typeparam name="TModel">The type of the model to validate.</typeparam>
        /// <param name="model">The model to validate.</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>A ValueTask representing the asynchronous validation operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no validator is registered for the model type.</exception>
        public async ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
        {
            var validator = _serviceProvider.GetService<IAsyncValidator<TModel>>();

            if(validator == null)
                throw new InvalidOperationException($"No validator registered for type {typeof(TModel).FullName}");

            var result = await validator.ValidateAsync(model, cancellationToken);

            if (!result.IsValid)
                throw new ModelValidationException(result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        }
    }
}
