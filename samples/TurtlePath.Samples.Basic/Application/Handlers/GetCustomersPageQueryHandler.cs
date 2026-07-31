using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class GetCustomersPageQueryHandler : GetPagedInfoQueryHandler<GetCustomersPageQuery, Customer, CustomerResponse>
{
    public GetCustomersPageQueryHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override string DefaultSorts => "Name";
}
