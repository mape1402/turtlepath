using TurtlePath.Domain.Contracts;

namespace TurtlePath.Samples.Basic.Domain.Entities;

public sealed class CatalogItem : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
