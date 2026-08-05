using TurtlePath.Models.Responses;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class CatalogItemResponse : BaseResponse
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
