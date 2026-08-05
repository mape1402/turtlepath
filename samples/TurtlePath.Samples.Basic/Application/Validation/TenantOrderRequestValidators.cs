using Crabalidator;
using TurtlePath.Samples.Basic.Application.Requests;

namespace TurtlePath.Samples.Basic.Application.Validation;

public sealed class CreateTenantOrderRequestValidator : CrabValidator<CreateTenantOrderRequest>
{
    public CreateTenantOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).Must(id => !id.IsEmpty);
        RuleFor(x => x.Total).Must(value => value > 0m);
    }
}

public sealed class DeleteTenantOrderRequestValidator : CrabValidator<DeleteTenantOrderRequest>
{
    public DeleteTenantOrderRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
    }
}

public sealed class CreateLegacyInvoiceRequestValidator : CrabValidator<CreateLegacyInvoiceRequest>
{
    public CreateLegacyInvoiceRequestValidator()
    {
        RuleFor(x => x.CustomerId).Must(id => !id.IsEmpty);
        RuleFor(x => x.Amount).Must(value => value > 0m);
    }
}

public sealed class UpdateLegacyInvoiceRequestValidator : CrabValidator<UpdateLegacyInvoiceRequest>
{
    public UpdateLegacyInvoiceRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
        RuleFor(x => x.CustomerId).Must(id => !id.IsEmpty);
        RuleFor(x => x.Amount).Must(value => value > 0m);
    }
}
