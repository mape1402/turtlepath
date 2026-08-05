using Pelican.Mediator;
using TurtlePath.Automations.Attributes;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Requests;

[DeleteAutomation(typeof(CatalogItem), typeof(DeletedResourceResponse))]
public sealed class DeleteCatalogItemRequest : BaseRequest, IRequest<DeletedResourceResponse>
{
}
