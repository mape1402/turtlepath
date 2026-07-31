using System.Text;
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

        builder.UseCIdFor<TenantOrder, CId, string>(config =>
        {
            config.DefaultFactory = () => CompositeOrderId.Create(Guid.Empty, 0);
            config.ConvertToDb = id => CompositeOrderId.ToStorage(id);
            config.ConvertFromDb = value => CompositeOrderId.FromStorage(value);
            config.JsonConverter = CompositeOrderId.FromStorage;
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : CompositeOrderId.FromStorage(value);
            config.ParseFunction = CompositeOrderId.FromStorage;
            config.ToByteArrayFunction = value => Encoding.UTF8.GetBytes(CompositeOrderId.ToStorage(value));
        });
    }
}
