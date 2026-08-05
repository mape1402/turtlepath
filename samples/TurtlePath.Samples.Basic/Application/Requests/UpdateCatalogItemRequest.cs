using Pelican.Mediator;
using TurtlePath.Automations.Attributes;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Requests;

[UpdateAutomation(typeof(CatalogItem), typeof(CatalogItemResponse))]
public sealed class UpdateCatalogItemRequest : BaseRequest, IRequest<CatalogItemResponse>
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
