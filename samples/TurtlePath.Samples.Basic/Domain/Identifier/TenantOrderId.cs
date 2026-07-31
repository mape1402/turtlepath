using System.Globalization;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Domain.Identifier;

public static class TenantOrderId
{
    public static CId Create(Guid tenantId, int orderNumber)
        => CId.From(new TenantOrderKey(tenantId, orderNumber));

    public static CId FromStorage(string value)
        => CId.From(Parse(value));

    public static TenantOrderKey Parse(string value)
    {
        var parts = value.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new FormatException("Tenant order id must use the 'tenantId|orderNumber' format.");

        return new TenantOrderKey(Guid.Parse(parts[0]), int.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    public static string ToStorage(CId id)
        => ToStorage(id.Cast<TenantOrderKey>());

    public static string ToStorage(TenantOrderKey key)
        => string.Create(CultureInfo.InvariantCulture, $"{key.TenantId}|{key.OrderNumber}");
}
