using TurtlePath.Models.Responses;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class LegacyShipmentResponse : IBaseResponse<int>
{
    public int Id { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}
