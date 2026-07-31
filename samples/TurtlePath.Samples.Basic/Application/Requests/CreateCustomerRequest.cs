namespace TurtlePath.Samples.Basic.Application.Requests;

using Pelican.Mediator;
using TurtlePath.Samples.Basic.Application.Responses;

public sealed record CreateCustomerRequest(string Name, string Email) : IRequest<CustomerResponse>;
