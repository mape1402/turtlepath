using System.Linq.Expressions;
using TurtlePath.Queries;
using TurtlePath.Samples.Basic.Application.Queries;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Handlers;

public sealed class GetCustomersQueryHandler : GetManyQueryHandler<GetCustomersQuery, Customer, CustomerResponse>
{
    public GetCustomersQueryHandler(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    protected override Expression<Func<Customer, bool>> GetFilterExpression(GetCustomersQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Search))
            return null;

        var search = query.Search.Trim().ToLowerInvariant();
        return customer => customer.Name.ToLower().Contains(search) || customer.Email.ToLower().Contains(search);
    }

    protected override Expression<Func<Customer, object>> GetSortingExpression(GetCustomersQuery query)
        => customer => customer.Name;
}
