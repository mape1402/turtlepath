using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Application.Requests;

using Pelican.Mediator;
using TurtlePath.Samples.Basic.Application.Responses;

public sealed record CreateTenantOrderRequest(CId CustomerId, decimal Total) : IRequest<TenantOrderResponse>;
