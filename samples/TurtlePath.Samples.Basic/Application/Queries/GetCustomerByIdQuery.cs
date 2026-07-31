using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;
using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Application.Queries;

public sealed class GetCustomerByIdQuery : GetByIdQuery<Customer, CustomerResponse>
{
    public GetCustomerByIdQuery(CId id) : base(id)
    {
    }
}
