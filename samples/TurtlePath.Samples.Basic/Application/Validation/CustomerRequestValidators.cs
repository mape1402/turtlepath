using Crabalidator;
using TurtlePath.Samples.Basic.Application.Requests;

namespace TurtlePath.Samples.Basic.Application.Validation;

public sealed class CreateCustomerRequestValidator : CrabValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(120).Must(value => value.Contains('@'));
    }
}

public sealed class UpdateCustomerRequestValidator : CrabValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(120).Must(value => value.Contains('@'));
    }
}

public sealed class PatchCustomerEmailRequestValidator : CrabValidator<PatchCustomerEmailRequest>
{
    public PatchCustomerEmailRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(120).Must(value => value.Contains('@'));
    }
}
