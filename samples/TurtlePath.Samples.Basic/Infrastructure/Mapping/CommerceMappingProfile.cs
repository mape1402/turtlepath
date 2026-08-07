using OctoMap;
using TurtlePath.Samples.Basic.Application.EventSourcing;
using TurtlePath.Samples.Basic.Application.Events;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Mapping;

public sealed class CommerceMappingProfile : OctoMapProfile
{
    public override void Configure(IOctoMapConfigurationBuilder builder)
    {
        builder.CreateMap<CreateCustomerRequest, Customer>();
        builder.CreateMap<UpdateCustomerRequest, Customer>();
        builder.CreateMap<Customer, CustomerResponse>();
        builder.CreateMap<CustomerEventSource, CustomerCreated>();
        builder.CreateMap<CustomerEventSource, CustomerUpdated>();
        builder.CreateMap<CustomerAuditEventSource, CustomerAuditEventRegistered>();
        builder.CreateMap<CustomerAuditEventSource, CustomerEmailPatched>();

        builder.CreateMap<CreateTenantOrderRequest, TenantOrder>();
        builder.CreateMap<TenantOrder, TenantOrderResponse>();

        builder.CreateMap<CreateLegacyInvoiceRequest, LegacyInvoice>();
        builder.CreateMap<UpdateLegacyInvoiceRequest, LegacyInvoice>();
        builder.CreateMap<LegacyInvoice, LegacyInvoiceResponse>();

        builder.CreateMap<CreateLegacyShipmentRequest, LegacyShipment>();
        builder.CreateMap<LegacyShipment, LegacyShipmentResponse>();

        builder.CreateMap<CreateCatalogItemRequest, CatalogItem>();
        builder.CreateMap<UpdateCatalogItemRequest, CatalogItem>();
        builder.CreateMap<CatalogItem, CatalogItemResponse>();
        builder.CreateMap<CatalogItem, DeletedResourceResponse>()
            .ForMember(x => x.Resource, x => x.MapFrom(_ => nameof(CatalogItem)));
    }
}
