using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class GetLegacyShipmentByIdQueryHandler
    : GenericGetByIdQueryHandler<GetLegacyShipmentByIdQuery, LegacyShipment, LegacyShipmentResponse, int>
{
    public GetLegacyShipmentByIdQueryHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
