using TurtlePath.Commands;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class CreateCustomerCommandHandler : CreateCommandHandler<CreateCustomerRequest, CustomerResponse, Customer>
{
    public CreateCustomerCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
