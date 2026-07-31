using System.Globalization;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Domain.Identifier;

public static class CompositeOrderId
{
    private const string TenantPart = "TenantId";
    private const string OrderPart = "OrderNumber";

    public static CId Create(Guid tenantId, int orderNumber)
        => CId.Composite(
            new CIdPart(TenantPart, tenantId),
            new CIdPart(OrderPart, orderNumber));

    public static CId FromStorage(string value)
    {
        var parts = value.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new FormatException("Composite order id must use the 'tenantId|orderNumber' format.");

        return Create(Guid.Parse(parts[0]), int.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    public static string ToStorage(CId id)
    {
        var tenantId = id.Parts.Single(part => part.Name == TenantPart).Value;
        var orderNumber = id.Parts.Single(part => part.Name == OrderPart).Value;

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{tenantId}|{orderNumber}");
    }
}
