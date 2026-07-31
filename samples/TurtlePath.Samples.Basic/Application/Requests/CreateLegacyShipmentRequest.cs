using Pelican.Mediator;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed record CreateLegacyShipmentRequest(int Id, string Carrier, string TrackingNumber) : IRequest<LegacyShipmentResponse>;
