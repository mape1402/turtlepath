using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class GetCustomerByIdQueryHandler : GetByIdQueryHandler<GetCustomerByIdQuery, Customer, CustomerResponse>
{
    public GetCustomerByIdQueryHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
