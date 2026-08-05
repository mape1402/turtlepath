using Pelican.Mediator;
using TurtlePath.Domain.Identifier;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed record CreateLegacyInvoiceRequest(CId CustomerId, decimal Amount) : IRequest<LegacyInvoiceResponse>;
