using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Queries;

public sealed class GetCustomersQuery : GetManyQuery<Customer, CustomerResponse>
{
    public string Search { get; init; } = string.Empty;
}
