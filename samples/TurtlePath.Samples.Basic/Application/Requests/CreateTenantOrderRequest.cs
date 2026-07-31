using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed record CreateTenantOrderRequest(CId CustomerId, Guid TenantId, int OrderNumber, decimal Total);
