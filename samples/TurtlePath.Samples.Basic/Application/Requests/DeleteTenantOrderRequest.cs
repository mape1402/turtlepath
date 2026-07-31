using Pelican.Mediator;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed class DeleteTenantOrderRequest : BaseRequest, IRequest<DeletedResourceResponse>
{
}
