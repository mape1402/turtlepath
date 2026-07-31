using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Queries;

public sealed class GetLegacyShipmentByIdQuery : GenericGetByIdQuery<LegacyShipment, LegacyShipmentResponse, int>
{
    public GetLegacyShipmentByIdQuery(int id) : base(id)
    {
    }
}
