using TurtlePath.Commands;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class CreateLegacyShipmentCommandHandler
    : GenericCreateCommandHandler<CreateLegacyShipmentRequest, LegacyShipmentResponse, LegacyShipment, int>
{
    public CreateLegacyShipmentCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
