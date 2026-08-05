using TurtlePath.Automations.Attributes;
using TurtlePath.Domain.Identifier;
using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Queries;

[GetByIdAutomation(typeof(CatalogItem), typeof(CatalogItemResponse))]
public sealed class GetCatalogItemByIdQuery : GetByIdQuery<CatalogItem, CatalogItemResponse>
{
    public GetCatalogItemByIdQuery(CId id) : base(id)
    {
    }
}
