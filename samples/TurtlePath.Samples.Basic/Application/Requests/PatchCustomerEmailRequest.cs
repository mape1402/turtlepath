using Pelican.Mediator;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed class PatchCustomerEmailRequest : BaseRequest, IRequest<CustomerResponse>
{
    public string Email { get; set; } = string.Empty;
}
