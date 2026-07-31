using TurtlePath.Domain.Contracts;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class LegacyShipment : IEntity<int>
{
    public int Id { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}
