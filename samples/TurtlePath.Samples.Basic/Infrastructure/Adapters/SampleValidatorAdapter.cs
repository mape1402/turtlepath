using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Validation;

namespace TurtlePath.Samples.Basic.Infrastructure.Adapters;

public sealed class SampleValidatorAdapter : IValidatorAdapter
{
    public ValueTask ValidateAsync<TModel>(TModel model, CancellationToken cancellationToken = default)
    {
        switch (model)
        {
            case CreateCustomerRequest request:
                Require(!string.IsNullOrWhiteSpace(request.Name), "Customer name is required.");
                Require(!string.IsNullOrWhiteSpace(request.Email), "Customer email is required.");
                break;
            case UpdateCustomerRequest request:
                Require(!request.Id.IsEmpty, "Customer id is required.");
                Require(!string.IsNullOrWhiteSpace(request.Name), "Customer name is required.");
                Require(!string.IsNullOrWhiteSpace(request.Email), "Customer email is required.");
                break;
            case PatchCustomerEmailRequest request:
                Require(!request.Id.IsEmpty, "Customer id is required.");
                Require(!string.IsNullOrWhiteSpace(request.Email), "Customer email is required.");
                break;
            case CreateTenantOrderRequest request:
                Require(request.Total > 0, "Order total must be greater than zero.");
                break;
            case CreateLegacyShipmentRequest request:
                Require(request.Id > 0, "Legacy shipment id must be greater than zero.");
                Require(!string.IsNullOrWhiteSpace(request.Carrier), "Carrier is required.");
                Require(!string.IsNullOrWhiteSpace(request.TrackingNumber), "Tracking number is required.");
                break;
        }

        return ValueTask.CompletedTask;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
