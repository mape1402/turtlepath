using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Queries;

public sealed class GetCustomersPageQuery : GetPagedInfoQuery<Customer, CustomerResponse>
{
    public GetCustomersPageQuery(PagedSettings pagedSettings) : base(pagedSettings)
    {
    }
}
