using Crabalidator;
using TurtlePath.Samples.Basic.Application.Requests;

namespace TurtlePath.Samples.Basic.Application.Validation;

public sealed class CreateCatalogItemRequestValidator : CrabValidator<CreateCatalogItemRequest>
{
    public CreateCatalogItemRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Price).Must(value => value > 0m);
    }
}

public sealed class UpdateCatalogItemRequestValidator : CrabValidator<UpdateCatalogItemRequest>
{
    public UpdateCatalogItemRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Price).Must(value => value > 0m);
    }
}

public sealed class DeleteCatalogItemRequestValidator : CrabValidator<DeleteCatalogItemRequest>
{
    public DeleteCatalogItemRequestValidator()
    {
        RuleFor(x => x.Id).Must(id => !id.IsEmpty);
    }
}
