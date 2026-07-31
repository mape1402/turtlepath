using TurtlePath.Models.Responses;

namespace TurtlePath.Samples.Basic.Application.Responses;

public sealed class CustomerResponse : BaseResponse
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
