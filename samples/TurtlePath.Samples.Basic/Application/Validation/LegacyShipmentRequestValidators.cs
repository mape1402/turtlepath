using Crabalidator;
using TurtlePath.Samples.Basic.Application.Requests;

namespace TurtlePath.Samples.Basic.Application.Validation;

public sealed class CreateLegacyShipmentRequestValidator : CrabValidator<CreateLegacyShipmentRequest>
{
    public CreateLegacyShipmentRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Carrier).NotEmpty().MaximumLength(80);
        RuleFor(x => x.TrackingNumber).NotEmpty().MaximumLength(80);
    }
}
