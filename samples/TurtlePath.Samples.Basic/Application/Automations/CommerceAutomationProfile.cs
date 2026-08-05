using TurtlePath.Automations.Profiles;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Automations;

public sealed class CommerceAutomationProfile : TurtlePathAutomationProfile
{
    public override void Configure(ITurtlePathAutomationBuilder builder)
    {
        builder.For<Customer>()
            .ToCreate<CreateCustomerRequest, CustomerResponse>()
            .ToUpdate<UpdateCustomerRequest, CustomerResponse>()
            .ToPatch<PatchCustomerEmailRequest, CustomerResponse>()
            .ToGetById<GetCustomerByIdQuery, CustomerResponse>()
            .ToGetPaged<GetCustomersPageQuery, CustomerResponse>(query => query.DefaultSort("Name"));

        builder.For<LegacyInvoice>()
            .ToCreate<CreateLegacyInvoiceRequest, LegacyInvoiceResponse>()
            .ToUpdate<UpdateLegacyInvoiceRequest, LegacyInvoiceResponse>()
            .ToGetById<GetLegacyInvoiceByIdQuery, LegacyInvoiceResponse>();
    }
}
