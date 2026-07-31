using Pelican.Mediator;
using TurtlePath.Models.Requests;
using TurtlePath.Samples.Basic.Application.Responses;

namespace TurtlePath.Samples.Basic.Application.Requests;

public sealed class UpdateCustomerRequest : BaseRequest, IRequest<CustomerResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
