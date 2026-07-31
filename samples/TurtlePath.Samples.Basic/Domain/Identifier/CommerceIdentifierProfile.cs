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

        builder.UseCIdFor<TenantOrder, TenantOrderKey, string>(config =>
        {
            config.DefaultFactory = () => TenantOrderId.Create(Guid.Empty, 0);
            config.ConvertToDb = id => TenantOrderId.ToStorage(id);
            config.ConvertFromDb = value => TenantOrderId.FromStorage(value);
            config.JsonConverter = TenantOrderId.FromStorage;
            config.NullableJsonConverter = value => string.IsNullOrWhiteSpace(value) ? null : TenantOrderId.FromStorage(value);
            config.ParseFunction = TenantOrderId.FromStorage;
            config.ToByteArrayFunction = value => Encoding.UTF8.GetBytes(TenantOrderId.ToStorage(value));
        });
    }
}
