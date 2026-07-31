using TurtlePath.Mapping;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Samples.Basic.Domain.Identifier;

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
            Customer customer when typeof(TDestination) == typeof(CustomerResponse) => new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email
            },
            CreateTenantOrderRequest request when typeof(TDestination) == typeof(TenantOrder) => new TenantOrder
            {
                Id = CompositeOrderId.Create(request.TenantId, request.OrderNumber),
                CustomerId = request.CustomerId,
                Total = request.Total
            },
            TenantOrder order when typeof(TDestination) == typeof(TenantOrderResponse) => new TenantOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Total = order.Total
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
        => throw new NotSupportedException("The sample only demonstrates create command handlers.");
}
