namespace TurtlePath.Samples.Basic.Domain.Identifier;

public readonly record struct TenantOrderKey(Guid TenantId, int OrderNumber)
{
    public override string ToString()
        => $"{TenantId}|{OrderNumber}";
}
