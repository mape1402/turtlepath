using Pelican.Mediator;
using TurtlePath.Automations.Attributes;
using TurtlePath.Samples.Basic.Application.Responses;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Application.Requests;

[CreateAutomation(typeof(CatalogItem), typeof(CatalogItemResponse))]
public sealed record CreateCatalogItemRequest(string Sku, string Name, decimal Price) : IRequest<CatalogItemResponse>;
