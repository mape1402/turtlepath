using TurtlePath.Mapping;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Infrastructure.Adapters;

public sealed class SampleMapperAdapter : IMapperAdapter
{
    public ValueTask<TDestination> MapAsync<TSource, TDestination>(
        TSource source,
        CancellationToken cancellationToken = default)
        where TSource : class
        where TDestination : class
    {
        object result = source switch
        {
            CreateCustomerRequest request when typeof(TDestination) == typeof(Customer) => new Customer
            {
                Name = request.Name,
                Email = request.Email
            },
            CreateLegacyShipmentRequest request when typeof(TDestination) == typeof(LegacyShipment) => new LegacyShipment
            {
                Id = request.Id,
                Carrier = request.Carrier,
                TrackingNumber = request.TrackingNumber
            },
            CreateCatalogItemRequest request when typeof(TDestination) == typeof(CatalogItem) => new CatalogItem
            {
                Sku = request.Sku,
                Name = request.Name,
                Price = request.Price
            },
            Customer customer when typeof(TDestination) == typeof(CustomerResponse) => new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            },
            CatalogItem item when typeof(TDestination) == typeof(CatalogItemResponse) => new CatalogItemResponse
            {
                Id = item.Id,
                Sku = item.Sku,
                Name = item.Name,
                Price = item.Price
            },
            CatalogItem item when typeof(TDestination) == typeof(DeletedResourceResponse) => new DeletedResourceResponse
            {
                Id = item.Id,
                Resource = nameof(CatalogItem)
            },
            CreateTenantOrderRequest request when typeof(TDestination) == typeof(TenantOrder) => new TenantOrder
            {
                CustomerId = request.CustomerId,
                Total = request.Total
            },
            TenantOrder order when typeof(TDestination) == typeof(TenantOrderResponse) => new TenantOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Total = order.Total
            },
            LegacyShipment shipment when typeof(TDestination) == typeof(LegacyShipmentResponse) => new LegacyShipmentResponse
            {
                Id = shipment.Id,
                Carrier = shipment.Carrier,
                TrackingNumber = shipment.TrackingNumber
            },
            _ => throw new InvalidOperationException($"No sample mapping exists from {typeof(TSource).Name} to {typeof(TDestination).Name}.")
        };

        return ValueTask.FromResult((TDestination)result);
    }

    public ValueTask UpdateMapAsync<TSource, TDestination>(
        TSource source,
        TDestination destination,
        CancellationToken cancellationToken = default)
        where TSource : class
        where TDestination : class
    {
        switch (source, destination)
        {
            case (UpdateCustomerRequest request, Customer customer):
                customer.Name = request.Name;
                customer.Email = request.Email.Trim().ToLowerInvariant();
                return ValueTask.CompletedTask;
            case (UpdateCatalogItemRequest request, CatalogItem item):
                item.Sku = request.Sku;
                item.Name = request.Name;
                item.Price = request.Price;
                return ValueTask.CompletedTask;
            default:
                throw new InvalidOperationException($"No sample update mapping exists from {typeof(TSource).Name} to {typeof(TDestination).Name}.");
        }
    }
}
