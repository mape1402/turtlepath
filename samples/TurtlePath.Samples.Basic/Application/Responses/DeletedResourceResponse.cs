using TurtlePath.Domain.Identifier;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class DeletedResourceResponse
{
    public CId Id { get; set; } = CId.Empty;
    public string Resource { get; set; } = string.Empty;
}
