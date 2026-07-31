using TurtlePath.Commands;
using TurtlePath.Samples.Basic.Application.Requests;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class UpdateCustomerCommandHandler : UpdateCommandHandler<UpdateCustomerRequest, CustomerResponse, Customer>
{
    public UpdateCustomerCommandHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
