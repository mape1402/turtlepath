using TurtlePath.Domain.Identifier;
using TurtlePath.Samples.Basic.Domain.Entities;

namespace TurtlePath.Samples.Basic.Domain.Identifier;

public sealed class CommerceIdentifierProfile : CIdProfile
{
    public override void Configure(CIdProfileBuilder builder)
    {
        builder.UseCIdFor<LegacyInvoice, int, int>(config =>
        {
            config.DefaultFactory = () => CId.From(10_001);
            config.ConvertToDb = id => id.Cast<int>();
            config.ConvertFromDb = value => CId.From(value);
            config.JsonConverter = value => CId.From(int.Parse(value));
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CId.From(int.Parse(value));
            config.ParseFunction = value => CId.From(int.Parse(value));
            config.ToByteArrayFunction = value => BitConverter.GetBytes(value);
        });
    }
}
